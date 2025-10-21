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
        Task AddReportAsync(string query, string type, Reporter reporter, string note);
        Task SaveAsync();
    }
}
