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

        public async Task AddReportAsync(string query, string type, string reporterName, string reporterEmail, string note)
        {
            var reporter = new Reporter
            {
                Name = reporterName,
                Email = reporterEmail
            };

            await _repository.AddReportAsync(query, type, reporter, note);
            await _repository.SaveAsync();
        }
    }
}
