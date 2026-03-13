using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public interface IReportRepository
    {
        Task<ReportCheck?> GetByQueryAsync(string query);
        Task<IEnumerable<ReportCheck>> GetAllAsync();

        // Phương thức trả về bool: true nếu báo cáo được thêm mới, false nếu Reporter đã báo cáo mục này trước đó.
        Task<bool> AddReportAsync(string query, string type, Reporter reporter, string note);

        // Chức năng Khiếu nại
        Task AddAppealAsync(Appeal appeal);
        Task<Appeal?> GetPendingAppealByQueryAndEmailAsync(string query, string email);

        // Chức năng Admin
        Task<IEnumerable<Appeal>> GetAllAppealsAsync();
        Task UpdateReportCheckVerdict(int reportCheckId, string newVerdict);
        Task UpdateAppealStatus(int appealId, string newStatus);

        Task SaveAsync();

        Task<User?> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);

        // Thêm hàm lưu vết giao dịch doanh thu
        Task AddTransactionAsync(Transaction transaction);

        // Thêm hàm lấy danh sách giao dịch để làm Dashboard Admin
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<IEnumerable<ReporterRankingDto>> GetTopReportersAsync(int count);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<Transaction?> GetTransactionByIdAsync(int id);
        public Task<decimal> GetTotalConfirmedRevenueAsync();
        public Task<decimal> GetMonthlyConfirmedRevenueAsync();
        Task UpdateTransactionAsync(Transaction transaction);
        Task<PaginatedList<Transaction>> GetTransactionsPagedAsync(int pageIndex, int pageSize);
        Task<PaginatedList<User>> GetUsersPagedAsync(int pageIndex, int pageSize);
        Task<PaginatedList<ReportCheck>> GetReportsPagedAsync(int pageIndex, int pageSize);
        Task<PaginatedList<Appeal>> GetAppealsPagedAsync(int pageIndex, int pageSize);
        Task<bool> HasPendingTransactionAsync(string email);
    }
}
