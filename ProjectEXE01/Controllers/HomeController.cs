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
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Check(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                TempData["Error"] = "Vui lòng nhập số điện thoại, STK hoặc link.";
                return RedirectToAction("Index");
            }

            var report = await _service.SearchAsync(query);
            var result = new Result
            {
                Query = query
            };

            if (report != null)
            {
                result.Found = true;
                result.Type = report.Type;
                result.Verdict = report.Verdict;
                result.ReportCount = report.ReportCount;
                result.CheckedAt = report.CheckedAt;
                result.Reporters = report.ReporterReportChecks?
                    .Select(rr => rr.Reporter.Name + " (" + rr.Reporter.Email + ")")
                    .ToList() ?? new List<string>();
            }
            else
            {
                result.Found = false;
            }

            return View("Result", result);
        }

        [HttpGet]
        public IActionResult SelectAction()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitReport(string query, string type, string reporterName, string reporterEmail, string note)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                TempData["Error"] = "Vui lòng nhập thông tin cần báo cáo.";
                return RedirectToAction("Report");
            }

            await _service.AddReportAsync(query, type, reporterName, reporterEmail, note);
            TempData["Success"] = "Báo cáo của bạn đã được gửi. Xin cảm ơn sự đóng góp!";
            return RedirectToAction("Index");
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

        public IActionResult Report()
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
