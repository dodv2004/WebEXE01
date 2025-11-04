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
    }
}
