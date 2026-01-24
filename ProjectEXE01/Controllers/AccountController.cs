using BLL.Services;
using DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using ProjectEXE01.Models; // Đảm bảo namespace này chứa các ViewModel của bạn
namespace ProjectEXE01.Controllers
{
    public class AccountController : Controller
    {
        private readonly IReportService _service;

        public AccountController(IReportService service)
        {
            _service = service;
        }

        #region ĐĂNG KÝ & XÁC THỰC OTP

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View(model);
            }

            // Gọi logic Register mới: Lưu User tạm và gửi OTP qua Email
            var (success, message) = await _service.RegisterAsync(model.Email, model.Password);

            if (success)
            {
                TempData["Message"] = message;
                // Chuyển sang trang nhập mã OTP, truyền kèm email để xác nhận
                return RedirectToAction("VerifyOTP", new { email = model.Email });
            }

            ViewBag.Error = message;
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyOTP(string email, string flow) // Thêm tham số flow vào đây
        {
            ViewBag.Email = email;
            ViewBag.Flow = flow; // BẮT BUỘC phải có dòng này để View nhận diện được luồng
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(string email, string otp, string flow)
        {
            if (string.IsNullOrEmpty(email))
            {
                // Nếu đang quên mật khẩu thì về trang Forgot, còn lại về Register
                return flow == "forgot" ? RedirectToAction("ForgotPassword") : RedirectToAction("Register");
            }

            // 1. Gọi Service để kiểm tra mã OTP có khớp và còn hạn không
            var (success, message) = await _service.VerifyOTPAsync(email, otp);

            if (!success)
            {
                ViewBag.Error = message;
                ViewBag.Email = email;
                return View(); // Ở lại trang nhập OTP nếu sai mã
            }

            // 2. PHÂN LUỒNG XỬ LÝ
            // Nếu flow là "forgot" (đến từ luồng Quên mật khẩu)
            if (flow == "forgot")
            {
                // Chuyển hướng sang trang ResetPassword, kèm theo email và mã để xác minh lớp nữa
                return RedirectToAction("ResetPassword", new { email = email, otp = otp });
            }

            // Nếu là luồng Đăng ký tài khoản thông thường
            TempData["Message"] = "Xác thực tài khoản thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        #endregion

        #region ĐĂNG NHẬP & ĐĂNG XUẤT

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _service.LoginAsync(model.Email, model.Password);

            if (success)
            {
                // LƯU SESSION: Để hệ thống nhận diện và tính lượt tra cứu
                HttpContext.Session.SetString("UserEmail", model.Email);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = message;
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa sạch thông tin đăng nhập
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region QUÊN MẬT KHẨU

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return View();

            var (success, message) = await _service.ForgotPasswordAsync(email);

            if (success)
            {
                TempData["Message"] = message;
                // Chuyển sang trang đặt lại mật khẩu với mã OTP
                return RedirectToAction("VerifyOTP", new { email = email, flow = "forgot" });
            }

            ViewBag.Error = message;
            return View();
        }

        // 1. Giao diện nhập mật khẩu mới
        [HttpGet]
        public IActionResult ResetPassword(string email, string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = email;
            ViewBag.OTP = otp;
            return View();
        }

        // 2. Xử lý lưu mật khẩu mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string otp, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                ViewBag.Email = email;
                ViewBag.OTP = otp;
                return View();
            }

            // Gọi hàm Service mà chúng ta đã viết trước đó
            var (success, message) = await _service.ResetPasswordWithOTPAsync(email, otp, newPassword);

            if (success)
            {
                TempData["Message"] = "Mật khẩu đã được cập nhật. Hãy đăng nhập lại!";
                return RedirectToAction("Login");
            }

            ViewBag.Error = message;
            ViewBag.Email = email;
            ViewBag.OTP = otp;
            return View();
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Lấy email từ Session
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            // GỌI QUA SERVICE CHỨ KHÔNG GỌI REPO
            var (success, message, user) = await _service.GetUserProfileAsync(email);

            if (!success || user == null)
            {
                TempData["Error"] = message;
                return RedirectToAction("Login");
            }

            return View(user);
        }
        // Controllers/AccountController.cs
        [HttpGet]
        public IActionResult UpgradeVip() => View();

        [HttpPost]
        public async Task<IActionResult> ProcessUpgrade()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            // Sau khi thanh toán thành công (giả lập), cập nhật VIP
            var (success, message) = await _service.UpgradeToVipAsync(email);

            if (success)
            {
                TempData["Message"] = message;
                return RedirectToAction("Profile"); // Về trang cá nhân để xem vương miện mới
            }

            ViewBag.Error = message;
            return View("UpgradeVip");
        }
    }
}