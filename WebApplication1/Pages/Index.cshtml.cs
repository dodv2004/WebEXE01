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

        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int PendingAppeals { get; set; }
        public int TotalEntities { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }

        // Bổ sung các thuộc tính về User
        public int TotalUsers { get; set; }
        public int VipUsers { get; set; }
        public int NormalUsers { get; set; }
        public double VipPercentage { get; set; }

        public async Task OnGetAsync()
        {
            TotalReports = await _reportService.GetReportCountAsync();
            PendingReports = await _reportService.GetPendingReportCountAsync();
            PendingAppeals = await _reportService.GetPendingAppealCountAsync();
            TotalEntities = TotalReports;

            // Xử lý Người dùng
            var allUsers = await _reportService.GetAllUsersAsync();
            if (allUsers != null)
            {
                TotalUsers = allUsers.Count();
                VipUsers = allUsers.Count(u => u.IsVip); // Giả định có property IsVip
                NormalUsers = TotalUsers - VipUsers;
                VipPercentage = TotalUsers > 0 ? Math.Round((double)VipUsers / TotalUsers * 100, 1) : 0;
            }

            // Xử lý Doanh thu
            var transactions = await _reportService.GetAllTransactionsAsync();
            if (transactions != null)
            {
                var confirmedTransactions = transactions.Where(t => t.Status == 1).ToList();
                TotalRevenue = confirmedTransactions.Sum(t => t.Amount);
                MonthlyRevenue = confirmedTransactions
                    .Where(t => t.PaymentDate.Month == DateTime.Now.Month && t.PaymentDate.Year == DateTime.Now.Year)
                    .Sum(t => t.Amount);
            }
        }
    }
}