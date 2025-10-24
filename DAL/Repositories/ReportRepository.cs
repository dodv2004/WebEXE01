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

            // Kiểm tra và sử dụng Reporter đã tồn tại
            var existingReporter = await _context.Reporters
                .FirstOrDefaultAsync(r => r.Email == reporter.Email) ?? reporter;
            if (existingReporter.Id == 0) // Nếu là Reporter mới
            {
                _context.Reporters.Add(existingReporter);
                await SaveAsync(); // Lưu Reporter trước khi tạo quan hệ
            }

            // Kiểm tra xem Reporter này đã báo cáo ReportCheck này chưa
            var existingRelation = await _context.ReporterReportChecks
                .FirstOrDefaultAsync(rr => rr.ReporterId == existingReporter.Id && rr.ReportCheckId == existingReportCheck.Id);

            if (existingRelation != null)
            {
                // Không cho phép báo cáo lại nếu đã tồn tại
                return; // Thoát phương thức, không thêm quan hệ mới
            }

            // Tăng ReportCount nhưng chỉ thêm quan hệ mới nếu chưa tồn tại
            existingReportCheck.ReportCount++;
            var relation = new ReporterReportCheck
            {
                ReporterId = existingReporter.Id,
                ReportCheckId = existingReportCheck.Id,
                Note = note
            };
            _context.ReporterReportChecks.Add(relation);

            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}