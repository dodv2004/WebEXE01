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
        [HttpGet]
        public IActionResult Index()
        {
            // Sử dụng ViewBag/TempData để truyền thông báo từ các RedirectToAction khác
            ViewBag.Message = TempData["Message"] as string;
            ViewBag.IsError = TempData["IsError"] as bool? ?? false;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Check(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                TempData["Message"] = "Vui lòng nhập thông tin tra cứu.";
                TempData["IsError"] = true;
                return RedirectToAction("Index");
            }

            // Gọi Service đã được tối ưu để trả về Result DTO
            var (success, message, result) = await _service.SearchAsync(query);

            if (!success)
            {
                // Xử lý các lỗi nghiệp vụ trong Service
                TempData["Message"] = message;
                TempData["IsError"] = true;
                return RedirectToAction("Index");
            }

            // Trả về View Result với DTO
            return View("Result", result);
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

        public IActionResult Ranking()
        {
            return View();
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