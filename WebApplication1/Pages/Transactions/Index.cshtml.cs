using BLL.Services;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAdmin.Pages.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly IReportService _reportService;

        public IndexModel(IReportService reportService)
        {
            _reportService = reportService;
        }
       
        // Property chứa danh sách giao dịch để View hiển thị
        public IEnumerable<Transaction> Transactions { get; set; } = new List<Transaction>();

        // Tự động chạy khi bạn truy cập trang
        public PaginatedList<Transaction> PagedData { get; set; }

        public async Task OnGetAsync(int p = 1) // Mặc định là trang 1
        {
            int pageSize = 5; // Mỗi trang 10 dòng
            PagedData = await _reportService.GetTransactionsPagedAsync(p, pageSize);
        }

        // Xử lý khi nhấn nút "Duyệt ngay" (asp-page-handler="Approve")
        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            // Cần đảm bảo bạn đã code hàm ApproveTransactionAsync trong Service
            var (success, message) = await _reportService.ApproveTransactionAsync(id);
            TempData["Message"] = message;

            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var (success, message) = await _reportService.RejectTransactionAsync(id);
            TempData["Message"] = message;

            return RedirectToPage();
        }
    }
}