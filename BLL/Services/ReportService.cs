using DAL.Models;
using DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;

        public ReportService(IReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<ReportCheck?> SearchAsync(string query)
        {
            return await _repository.GetByQueryAsync(query);
        }

        public async Task<IEnumerable<ReportCheck>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<(bool Success, string Message)> AddReportAsync(string query, string type, string reporterName, string reporterEmail, string note)
        {
            var reporter = new Reporter
            {
                Name = reporterName,
                Email = reporterEmail
            };

            await _repository.AddReportAsync(query, type, reporter, note);
            // Kiểm tra xem quan hệ đã được thêm thành công
            var existingRelation = await _repository.GetByQueryAsync(query);
            if (existingRelation?.ReporterReportChecks?.Any(rr => rr.Reporter.Email == reporterEmail) ?? false)
            {
                return (true, "Báo cáo của bạn đã được gửi thành công!");
            }
            return (false, "Bạn đã báo cáo mục này trước đó. Không thể báo cáo lại!");
        }
    }
}
