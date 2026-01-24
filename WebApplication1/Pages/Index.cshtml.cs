using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebAdmin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IReportService _reportService;

        public IndexModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        // Properties để lưu trữ số liệu thống kê
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int PendingAppeals { get; set; }
        public int TotalEntities { get; set; } // Tổng số mục (Query) trong DB
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalUsers { get; set; }
        public async Task OnGetAsync()
        {
            // Gọi các phương thức thống kê từ BLL
            TotalReports = await _reportService.GetReportCountAsync();
            PendingReports = await _reportService.GetPendingReportCountAsync();
            PendingAppeals = await _reportService.GetPendingAppealCountAsync();

            // Hiện tại TotalEntities bằng TotalReports, nhưng có thể khác sau này
            TotalEntities = TotalReports;
            // Lấy danh sách giao dịch để tính tiền
            var transactions = await _reportService.GetAllTransactionsAsync();
            if (transactions != null)
            {
                TotalRevenue = transactions.Sum(t => t.Amount);
                MonthlyRevenue = transactions
                    .Where(t => t.PaymentDate.Month == DateTime.Now.Month && t.PaymentDate.Year == DateTime.Now.Year)
                    .Sum(t => t.Amount);
            }
            var allUsers = await _reportService.GetAllUsersAsync();
            TotalUsers = allUsers.Count();
        }
    }
}
