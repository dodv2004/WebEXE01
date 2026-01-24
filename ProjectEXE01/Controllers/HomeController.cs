using BLL.Services;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE01.Models;
using System.Diagnostics;

namespace ProjectEXE01.Controllers
{
    public class HomeController : Controller
    {
        private readonly IReportService _service;

        public HomeController(IReportService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(userEmail))
            {
                // Lấy thông tin user từ Service
                var (success, message, user) = await _service.GetUserProfileAsync(userEmail);
                if (success && user != null)
                {
                    ViewBag.IsVip = user.IsVip;
                    ViewBag.RemainingSearches = 15 - user.SearchCount;
                }
            }

            ViewBag.Message = TempData["Message"] as string;
            ViewBag.IsError = TempData["IsError"] as bool? ?? false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Check(string query)
        {
            // 1. Kiểm tra session đăng nhập
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Message"] = "Vui lòng đăng nhập để sử dụng tính năng tra cứu.";
                TempData["IsError"] = true;
                return RedirectToAction("Index");
            }

            // 2. Kiểm tra chuỗi nhập vào trước khi làm bất cứ việc gì
            if (string.IsNullOrWhiteSpace(query))
            {
                TempData["Message"] = "Vui lòng nhập số điện thoại hoặc STK cần kiểm tra.";
                TempData["IsError"] = true;
                return RedirectToAction("Index");
            }

            // 3. KIỂM TRA QUYỀN (Không tăng lượt ở bước này)
            // Hàm này sẽ check xem user là VIP hay đã hết 15 lượt chưa
            var permission = await _service.CheckSearchPermissionAsync(userEmail);
            if (!permission.CanSearch)
            {
                TempData["Message"] = permission.Message;
                TempData["IsError"] = true;
                return RedirectToAction("Index");
            }

            // 4. THỰC HIỆN TRA CỨU
            var (success, message, result) = await _service.SearchAsync(query);

            if (!success)
            {
                TempData["Message"] = message;
                TempData["IsError"] = true;
                return RedirectToAction("Index");
            }

            // 5. CẬP NHẬT LƯỢT DÙNG (Chỉ chạy khi tra cứu hợp lệ)
            // Hàm IncrementSearchCountAsync sẽ tự check nếu là VIP thì KHÔNG tăng lượt
            await _service.IncrementSearchCountAsync(userEmail);

            // 6. TRẢ VỀ KẾT QUẢ
            // Nếu tìm thấy scammer, chuyển sang trang Result. Nếu không, báo tin vui ở Index
            if (result != null && result.Found)
            {
                return View("Result", result);
            }
            else
            {
                TempData["Message"] = "Thông tin này hiện chưa có trong danh sách đen. Hãy luôn cảnh giác!";
                TempData["IsError"] = false;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Report()
        {

            ViewBag.Message = TempData["Message"] as string;
            ViewBag.IsError = TempData["IsError"] as bool? ?? false;
            return View(new ReportViewModel());
        }

        [HttpGet]
        public IActionResult SelectAction()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitReport(ReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // KHI KHÔNG HỢP LỆ (Không có Redirect, dùng ViewBag)
                ViewBag.Message = "Vui lòng kiểm tra lại thông tin nhập vào.";
                ViewBag.IsError = true;
                return View("Report", model);
            }

            try
            {
                var (success, message) = await _service.AddReportAsync(
                    model.Query, model.Type, model.ReporterName, model.ReporterEmail, model.Note
                );

                // KHI THÀNH CÔNG/LỖI NGHIỆP VỤ (Có Redirect, dùng TempData)
                TempData["Message"] = message;
                TempData["IsError"] = !success;
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Có lỗi hệ thống xảy ra. Vui lòng thử lại sau. (" + ex.Message + ")";
                TempData["IsError"] = true;
            }

            return RedirectToAction("Report");
        }


        [HttpGet]
        public ActionResult Appeal(string query = "")
        {
            ViewBag.Message = TempData["Message"] as string;
            ViewBag.IsError = TempData["IsError"] as bool? ?? false;

            // Truyền Query nếu có từ trang Result
            return View(new AppealViewModel { Query = query });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitAppeal(AppealViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // KHI KHÔNG HỢP LỆ (Không có Redirect, dùng ViewBag)
                ViewBag.Message = "Vui lòng kiểm tra lại thông tin nhập vào.";
                ViewBag.IsError = true;
                return View("Appeal", model);
            }

            var (success, message) = await _service.SubmitAppealAsync(
                model.Query, model.AppellantName, model.AppellantEmail, model.Reason
            );

            // KHI THÀNH CÔNG/LỖI NGHIỆP VỤ (Có Redirect, dùng TempData)
            TempData["Message"] = message;
            TempData["IsError"] = !success;

            return RedirectToAction("Appeal");
        }


        public IActionResult News()
        {
            return View();
        }

        public IActionResult Resources()
        {
            return View();
        }

        public async Task<IActionResult> Ranking()
        {
            // Lấy dữ liệu từ Service
            var topReporters = await _service.GetTopReportersAsync(10);

            // QUAN TRỌNG: Nếu topReporters bị null, hãy truyền một danh sách rỗng để View không bị crash
            return View(topReporters ?? new List<ReporterRankingDto>());
        }

        public IActionResult Media()
        {
            return View();
        }

        
        public IActionResult History()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}