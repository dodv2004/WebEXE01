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

namespace WebAdmin.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly IReportService _reportService;

        public IndexModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        // Đổi tên ReportCheck thành ReportList để thống nhất
        public IList<ReportCheck> ReportList { get; set; } = new List<ReportCheck>();

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public PaginatedList<ReportCheck> PagedData { get; set; }

        public async Task OnGetAsync(int p = 1)
        {
            // Lấy dữ liệu phân trang từ Service (mỗi trang 10 báo cáo)
            PagedData = await _reportService.GetReportsPagedAsync(p, 5);
        }

        // --- HANDLER CHO VIỆC XÁC MINH TRẠNG THÁI (VERDICT) ---
        public async Task<IActionResult> OnPostUpdateVerdictAsync(int id, string verdict)
        {
            if (string.IsNullOrEmpty(verdict))
            {
                StatusMessage = "Error: Vui lòng chọn trạng thái xác minh.";
                return RedirectToPage();
            }

            // Gọi Service để cập nhật Verdict chính thức
            var (success, message) = await _reportService.UpdateReportVerdictAsync(id, verdict);

            StatusMessage = success ? ("Success: " + message) : ("Error: " + message);

            return RedirectToPage();
        }
    }
}
