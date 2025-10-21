using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ProjectEXE01Context _context;

        public ReportRepository(ProjectEXE01Context context)
        {
            _context = context;
        }

        // Lấy thông tin theo số điện thoại / STK / link
        public async Task<ReportCheck?> GetByQueryAsync(string query)
        {
            return await _context.ReportChecks
                .Include(rc => rc.ReporterReportChecks)
                    .ThenInclude(rr => rr.Reporter)
                .FirstOrDefaultAsync(rc => rc.Query == query);
        }

        public async Task<IEnumerable<ReportCheck>> GetAllAsync()
        {
            return await _context.ReportChecks
                .Include(rc => rc.ReporterReportChecks)
                .ThenInclude(rr => rr.Reporter)
                .OrderByDescending(rc => rc.CheckedAt)
                .ToListAsync();
        }

        // Thêm 1 báo cáo mới (nếu chưa có thì tạo mới ReportCheck)
        public async Task AddReportAsync(string query, string type, Reporter reporter, string note)
        {
            var existingReportCheck = await _context.ReportChecks
                .Include(rc => rc.ReporterReportChecks)
                .FirstOrDefaultAsync(rc => rc.Query == query);

            if (existingReportCheck == null)
            {
                existingReportCheck = new ReportCheck
                {
                    Query = query,
                    Type = type,
                    Verdict = "Chưa xác định",
                    ReportCount = 0
                };
                _context.ReportChecks.Add(existingReportCheck);
            }

            existingReportCheck.ReportCount++;

            // Kiểm tra xem người báo cáo đã tồn tại chưa
            var existingReporter = await _context.Reporters.FirstOrDefaultAsync(r => r.Email == reporter.Email);
            if (existingReporter == null)
            {
                existingReporter = reporter;
                _context.Reporters.Add(existingReporter);
            }

            // Thêm bản ghi vào bảng trung gian
            var relation = new ReporterReportCheck
            {
                Reporter = existingReporter,
                ReportCheck = existingReportCheck,
                Note = note
            };

            _context.ReporterReportChecks.Add(relation);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
