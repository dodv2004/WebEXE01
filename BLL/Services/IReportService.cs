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
        Task<ReportCheck?> SearchAsync(string query);
        Task<IEnumerable<ReportCheck>> GetAllAsync();
        Task<(bool Success, string Message)> AddReportAsync(string query, string type, string reporterName, string reporterEmail, string note);
    }
}
