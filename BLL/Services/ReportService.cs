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

        public async Task<(bool Success, string Message, Result? ResultData)> SearchAsync(string query)
        {
            var cleanQuery = query.ToLower().Trim();
            var reportCheck = await _repository.GetByQueryAsync(cleanQuery);

            if (reportCheck == null)
            {
                // Không tìm thấy trong DB
                return (true, "Không tìm thấy báo cáo nào liên quan. Vui lòng cẩn thận khi giao dịch!",
                        new Result { Query = query, Found = false, Verdict = "An toàn (Chưa có dữ liệu)" });
            }

            const int WARNING_THRESHOLD = 5; // Ngưỡng cảnh báo có thể cấu hình

            if (reportCheck.ReportCount >= WARNING_THRESHOLD && reportCheck.Verdict.Contains("Chờ xác minh"))
            {

                // Cập nhật lại Verdict nếu số lượng báo cáo quá cao
                reportCheck.Verdict = $"CẢNH BÁO CAO: Đã bị báo cáo {reportCheck.ReportCount} lần. RẤT NGUY HIỂM!";
            }

            // Chuyển đổi ReportCheck Entity sang Result DTO
            var result = new Result
            {
                Query = reportCheck.Query,
                Found = true,
                Type = reportCheck.Type,
                Verdict = reportCheck.Verdict,
                ReportCount = reportCheck.ReportCount,
                CheckedAt = reportCheck.CheckedAt,
                // Chỉ lấy Tên Reporter (giảm thiểu rò rỉ thông tin cá nhân)
                Reporters = reportCheck.ReporterReportChecks?
                    .Select(rr => rr.Reporter.Name)
                    .Distinct()
                    .ToList() ?? new List<string>()
            };

            return (true, "Đã tìm thấy báo cáo liên quan!", result);
        }

        public async Task<IEnumerable<ReportCheck>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<(bool Success, string Message)> AddReportAsync(string query, string type, string reporterName, string reporterEmail, string note)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return (false, "Thông tin báo cáo không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(reporterEmail))
            {
                return (false, "Email người báo cáo không được để trống.");
            }

            var reporter = new Reporter
            {
                Name = reporterName,
                Email = reporterEmail
            };

            // Gọi repository và nhận kết quả
            var isNewReport = await _repository.AddReportAsync(query, type, reporter, note ?? string.Empty);

            if (isNewReport)
            {
                return (true, "Báo cáo của bạn đã được gửi thành công và đang chờ xác minh từ Admin.");
            }

            // Nếu isNewReport là false, tức là đã tồn tại
            return (false, "Bạn đã báo cáo mục này trước đó. Cảm ơn bạn đã hợp tác!");
        }

        // --- CHỨC NĂNG KHIẾU NẠI (MỚI) ---

        public async Task<(bool Success, string Message)> SubmitAppealAsync(string query, string name, string email, string reason)
        {
            var cleanQuery = query.ToLower().Trim();
            var cleanEmail = email.ToLower().Trim();

            // 1. Kiểm tra Query có tồn tại trong danh sách báo cáo (ReportCheck) không?
            var existingReportCheck = await _repository.GetByQueryAsync(cleanQuery);
            if (existingReportCheck == null)
            {
                return (false, $"Mục '{query}' chưa từng bị báo cáo trên hệ thống. Vui lòng kiểm tra lại thông tin!");
            }

            // 2. Kiểm tra khiếu nại trùng lặp (1 người - 1 Query - 1 khiếu nại Pending)
            var pendingAppeal = await _repository.GetPendingAppealByQueryAndEmailAsync(cleanQuery, cleanEmail);

            if (pendingAppeal != null)
            {
                return (false, "Bạn đã có một yêu cầu khiếu nại đang chờ Admin xử lý cho mục này. Vui lòng kiên nhẫn!");
            }

            // 3. Tạo và lưu khiếu nại mới
            var appeal = new Appeal
            {
                Query = query,
                AppellantName = name,
                AppellantEmail = email, // Repository sẽ chuẩn hóa
                Reason = reason
            };

            try
            {
                await _repository.AddAppealAsync(appeal);
                return (true, "Yêu cầu khiếu nại của bạn đã được gửi thành công. Admin sẽ xem xét và phản hồi qua email sớm nhất.");
            }
            catch
            {
                return (false, "Đã xảy ra lỗi hệ thống trong quá trình gửi khiếu nại. Vui lòng thử lại.");
            }
        }


        // --- CHỨC NĂNG ADMIN - THỐNG KÊ ---

        public async Task<int> GetReportCountAsync()
        {
            return (await _repository.GetAllAsync()).Count(); // Tổng số ReportCheck
        }

        public async Task<int> GetPendingReportCountAsync()
        {
            // Đếm các ReportCheck có Verdict là 'Chờ xác minh'
            return (await _repository.GetAllAsync())
                .Count(r => r.Verdict.Contains("Chờ xác minh"));
        }

        public async Task<int> GetPendingAppealCountAsync()
        {
            // Đếm các Appeal có Status là 'Pending'
            return (await _repository.GetAllAppealsAsync())
                .Count(a => a.Status == "Pending");
        }

        // --- CHỨC NĂNG ADMIN - XỬ LÝ ---

        public async Task<IEnumerable<ReportCheck>> GetAllReportsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<Appeal>> GetPendingAppealsAsync()
        {
            // Lấy tất cả và lọc ở BLL để duy trì tính nhất quán của Repo
            return (await _repository.GetAllAppealsAsync())
                .Where(a => a.Status == "Pending");
        }

        public async Task<(bool Success, string Message)> UpdateReportVerdictAsync(int reportCheckId, string newVerdict)
        {
            try
            {
                await _repository.UpdateReportCheckVerdict(reportCheckId, newVerdict);
                return (true, $"Đã cập nhật trạng thái báo cáo (ID: {reportCheckId}) thành '{newVerdict}'.");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi hệ thống khi cập nhật trạng thái: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> ProcessAppealAsync(int appealId, bool acceptAppeal)
        {
            var allAppeals = await _repository.GetAllAppealsAsync();
            var appeal = allAppeals.FirstOrDefault(a => a.Id == appealId);

            if (appeal == null) { return (false, "Không tìm thấy yêu cầu khiếu nại này."); }
            if (appeal.Status != "Pending") { return (false, "Khiếu nại này đã được xử lý trước đó."); }

            try
            {
                if (acceptAppeal)
                {
                    // 1. Gỡ trạng thái lừa đảo (Set Verdict về "An toàn")
                    if (appeal.ReportCheckId.HasValue)
                    {
                        await _repository.UpdateReportCheckVerdict(appeal.ReportCheckId.Value, "An toàn (Đã được Admin xác minh sau khiếu nại)");
                    }
                    // 2. Cập nhật Status Appeal
                    await _repository.UpdateAppealStatus(appealId, "Accepted");
                    return (true, "Khiếu nại đã được chấp nhận. Mục báo cáo đã được chuyển về trạng thái An toàn.");
                }
                else
                {
                    // 1. Cập nhật Status Appeal
                    await _repository.UpdateAppealStatus(appealId, "Rejected");
                    // 2. Không làm gì với ReportCheck
                    return (true, "Khiếu nại đã bị từ chối.");
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi hệ thống khi xử lý khiếu nại: " + ex.Message);
            }
        }
    }
}
