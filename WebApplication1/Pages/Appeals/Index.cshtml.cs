using BLL.Services;
using DAL.Data;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAdmin.Pages.Appeals
{
    public class IndexModel : PageModel
    {
        private readonly IReportService _reportService;

        public IndexModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        // Đổi tên Appeal thành AppealList để tránh nhầm lẫn với class (dùng IList)
        public IList<Appeal> AppealList { get; set; } = new List<Appeal>();

        // Dùng để hiển thị thông báo xử lý thành công/thất bại
        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            // GỌI SERVICE ĐỂ LẤY DỮ LIỆU (đã có Include ReportCheck trong Service)
            // Lấy các khiếu nại đang chờ xử lý
            AppealList = (await _reportService.GetPendingAppealsAsync())
                .OrderByDescending(a => a.SubmittedAt)
                .ToList();
        }

        // --- HANDLERS XỬ LÝ HÀNH ĐỘNG CỦA ADMIN ---

        public async Task<IActionResult> OnPostAcceptAsync(int id)
        {
            // Gọi Service để xử lý nghiệp vụ chấp nhận khiếu nại (acceptAppeal: true)
            var (success, message) = await _reportService.ProcessAppealAsync(id, acceptAppeal: true);

            StatusMessage = success ? ("Success: " + message) : ("Error: " + message);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            // Gọi Service để xử lý nghiệp vụ từ chối khiếu nại (acceptAppeal: false)
            var (success, message) = await _reportService.ProcessAppealAsync(id, acceptAppeal: false);

            StatusMessage = success ? ("Success: " + message) : ("Error: " + message);

            return RedirectToPage();
        }
    }
}
