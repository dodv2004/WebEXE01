using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IReportService
    {
        Task<(bool Success, string Message, Result? ResultData)> SearchAsync(string query);
        Task<IEnumerable<ReportCheck>> GetAllAsync();
        Task<(bool Success, string Message)> AddReportAsync(string query, string type, string reporterName, string reporterEmail, string note);

        Task<(bool Success, string Message)> SubmitAppealAsync(string query, string name, string email, string reason);

        // Chức năng Admin - Thống kê
        Task<int> GetReportCountAsync();
        Task<int> GetPendingReportCountAsync();
        Task<int> GetPendingAppealCountAsync();

        // Chức năng Admin - Xử lý
        Task<IEnumerable<ReportCheck>> GetAllReportsAsync();
        Task<IEnumerable<Appeal>> GetPendingAppealsAsync();
        Task<(bool Success, string Message)> UpdateReportVerdictAsync(int reportCheckId, string newVerdict);
        Task<(bool Success, string Message)> ProcessAppealAsync(int appealId, bool acceptAppeal);
    }
}
