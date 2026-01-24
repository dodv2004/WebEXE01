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
        private readonly EmailService _emailService;
        public ReportService(IReportRepository repository, EmailService emailService)
        {
            _repository = repository;
            _emailService = emailService;
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
        public async Task<(bool Success, string Message)> RegisterAsync(string email, string password)
        {
            var existing = await _repository.GetUserByEmailAsync(email);

            // Nếu đã tồn tại và đã kích hoạt
            if (existing != null && existing.IsActive) return (false, "Email đã được đăng ký!");

            var otp = GenerateOTP();
            if (existing == null)
            {
                // Tạo người dùng mới ở trạng thái IsActive = false
                var user = new User
                {
                    Email = email,
                    Password = password,
                    OTP = otp,
                    OTPExpiry = DateTime.Now.AddMinutes(5),
                    IsActive = false
                };
                await _repository.AddUserAsync(user);
            }
            else
            {
                // Cập nhật lại OTP cho tài khoản chưa kích hoạt
                existing.Password = password;
                existing.OTP = otp;
                existing.OTPExpiry = DateTime.Now.AddMinutes(5);
                await _repository.UpdateUserAsync(existing);
            }

            await _emailService.SendOTPAsync(email, otp);
            return (true, "Mã OTP đã được gửi đến email của bạn. Vui lòng xác thực để hoàn tất đăng ký.");
        }

        public async Task<(bool Success, string Message)> LoginAsync(string email, string password)
        {
            var user = await _repository.GetUserByEmailAsync(email);
            if (user == null || user.Password != password) return (false, "Email hoặc mật khẩu không chính xác.");

            // Kiểm tra xem đã xác thực OTP chưa
            if (!user.IsActive) return (false, "Tài khoản chưa được kích hoạt. Vui lòng xác thực mã OTP.");

            return (true, "Đăng nhập thành công!");
        }

        public async Task<(bool CanSearch, string Message)> CheckAndIncrementSearchLimitAsync(string email)
        {
            var user = await _repository.GetUserByEmailAsync(email);
            if (user == null) return (false, "Vui lòng đăng nhập để tra cứu.");

            // Tự động reset lượt nếu sang tháng mới (Lazy Reset)
            if (user.LastResetDate.Month != DateTime.Now.Month || user.LastResetDate.Year != DateTime.Now.Year)
            {
                user.SearchCount = 0;
                user.LastResetDate = DateTime.Now;
            }

            if (user.IsVip) return (true, "VIP"); // VIP không giới hạn

            if (user.SearchCount >= 15)
                return (false, "Bạn đã hết 15 lượt miễn phí tháng này. Hãy nâng cấp VIP!");

            // Nếu còn lượt, tăng số lượt dùng
            user.SearchCount++;
            await _repository.UpdateUserAsync(user);

            return (true, $"Bạn còn {15 - user.SearchCount} lượt tra cứu.");
        }
        public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _repository.GetUserByEmailAsync(email);
            if (user == null) return (false, "Email không tồn tại trong hệ thống.");

            user.Password = newPassword;
            await _repository.UpdateUserAsync(user);
            return (true, "Đặt lại mật khẩu thành công!");
        }
        // 1. Hàm tạo mã OTP 6 số ngẫu nhiên
        private string GenerateOTP() => new Random().Next(100000, 999999).ToString();

        // 2. Chỉnh sửa hàm Register (Đăng ký xong chưa cho dùng ngay, phải chờ OTP)
        public async Task<(bool Success, string Message)> RegisterWithOTPAsync(string email)
        {
            var otp = GenerateOTP();
            var user = await _repository.GetUserByEmailAsync(email);

            if (user == null)
            {
                user = new User { Email = email, OTP = otp, OTPExpiry = DateTime.Now.AddMinutes(5) };
                await _repository.AddUserAsync(user);
            }
            else
            {
                user.OTP = otp;
                user.OTPExpiry = DateTime.Now.AddMinutes(5);
                await _repository.UpdateUserAsync(user);
            }

            await _emailService.SendOTPAsync(email, otp); // Gửi mail
            return (true, "Mã OTP đã được gửi!");
        }

        // 3. Hàm xác nhận OTP
        public async Task<(bool Success, string Message)> VerifyOTPAsync(string email, string otp)
        {
            var user = await _repository.GetUserByEmailAsync(email);
            if (user == null || user.OTP != otp || user.OTPExpiry < DateTime.Now)
                return (false, "Mã OTP không đúng hoặc đã hết hạn.");

            user.IsActive = true; // Kích hoạt tài khoản
            user.OTP = null; // Xóa mã cũ
            await _repository.UpdateUserAsync(user);
            return (true, "Xác thực thành công!");
        }

        // Bước 1: Yêu cầu quên mật khẩu - Gửi OTP
        public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
        {
            var user = await _repository.GetUserByEmailAsync(email);
            if (user == null) return (false, "Email không tồn tại trên hệ thống.");

            var otp = GenerateOTP();
            user.OTP = otp;
            user.OTPExpiry = DateTime.Now.AddMinutes(5);

            await _repository.UpdateUserAsync(user);
            await _emailService.SendOTPAsync(email, otp);

            return (true, "Mã OTP đặt lại mật khẩu đã được gửi!");
        }

        // Bước 2: Reset mật khẩu với mã OTP
        public async Task<(bool Success, string Message)> ResetPasswordWithOTPAsync(string email, string otp, string newPassword)
        {
            var user = await _repository.GetUserByEmailAsync(email);

            if (user == null || user.OTP != otp || user.OTPExpiry < DateTime.Now)
                return (false, "Mã OTP không đúng hoặc đã hết hạn.");

            user.Password = newPassword;
            user.OTP = null; // Xóa mã OTP sau khi dùng xong
            user.IsActive = true; // Đảm bảo tài khoản được kích hoạt nếu trước đó chưa active

            await _repository.UpdateUserAsync(user);
            return (true, "Mật khẩu của bạn đã được cập nhật thành công!");
        }

        // Thêm vào ReportService.cs
        public async Task<(bool Success, string Message, User? UserData)> GetUserProfileAsync(string email)
        {
            var user = await _repository.GetUserByEmailAsync(email);
            if (user == null)
                return (false, "Không tìm thấy thông tin người dùng.", null);

            return (true, "Lấy thông tin thành công.", user);
        }

        public async Task<(bool CanSearch, string Message)> CheckSearchPermissionAsync(string email)
        {
            var user = await _repository.GetUserByEmailAsync(email);
            if (user == null) return (false, "Không tìm thấy thông tin tài khoản.");

            // --- BƯỚC KIỂM TRA HẾT HẠN VIP (THÊM VÀO ĐÂY) ---
            if (user.IsVip && user.VipExpiryDate.HasValue && user.VipExpiryDate < DateTime.Now)
            {
                user.IsVip = false; // "Hạ cấp" xuống tài khoản thường
                user.VipExpiryDate = null;

                // Lưu thay đổi xuống Database thông qua Repository
                await _repository.UpdateUserAsync(user);

                // Sau khi hết VIP, vẫn cho họ tra cứu nếu lượt dùng < 15
                if (user.SearchCount >= 15)
                {
                    return (false, "Gói VIP của bạn đã hết hạn và bạn cũng đã dùng hết 15 lượt miễn phí.");
                }

                // Trả về true nhưng kèm thông báo nhắc nhở
                return (true, "Gói VIP của bạn đã hết hạn. Bạn đang sử dụng 15 lượt tra cứu miễn phí.");
            }

            // --- LOGIC KIỂM TRA VIP CÒN HẠN ---
            if (user.IsVip)
            {
                return (true, "VIP");
            }

            // --- LOGIC CHO TÀI KHOẢN THƯỜNG ---
            if (user.SearchCount >= 15)
            {
                return (false, "Bạn đã hết 15 lượt tra cứu miễn phí tháng này. Hãy nâng cấp VIP!");
            }

            return (true, "Hợp lệ");
        }

        public async Task IncrementSearchCountAsync(string email)
        {
            var user = await _repository.GetUserByEmailAsync(email);
            if (user != null && !user.IsVip) // Chỉ xử lý nếu KHÔNG phải VIP
            {
                user.SearchCount++;
                await _repository.UpdateUserAsync(user);
            }
        }

        // Trong file ReportService.cs
        public async Task<(bool Success, string Message)> UpgradeToVipAsync(string email)
        {
            // 1. Tìm người dùng trong Database
            var user = await _repository.GetUserByEmailAsync(email);
            if (user == null) return (false, "Không tìm thấy thông tin tài khoản.");

            // 2. Xác định thời điểm bắt đầu tính hạn VIP
            // Nếu vẫn còn VIP, ta tính từ ngày hết hạn cũ. Nếu đã hết hoặc chưa có, tính từ Now.
            DateTime startDate = (user.IsVip && user.VipExpiryDate.HasValue && user.VipExpiryDate > DateTime.Now)
                                 ? user.VipExpiryDate.Value
                                 : DateTime.Now;

            // 3. Cập nhật trạng thái và cộng thêm chính xác 1 tháng
            user.IsVip = true;
            user.VipExpiryDate = startDate.AddMonths(1); // Tự động xử lý tháng 28, 30 hoặc 31 ngày
            var bill = new Transaction
            {
                UserId = user.Id,
                Email = user.Email,
                Amount = 89000, //
                TransactionNote = $"Nâng cấp VIP cho {user.Email}"
            };
            try
            {
                await _repository.AddTransactionAsync(bill);
                // 4. Lưu vào SQL Server thông qua Repository
                await _repository.UpdateUserAsync(user);

                return (true, $"Nâng cấp VIP thành công! Hạn dùng đến: {user.VipExpiryDate.Value:dd/MM/yyyy HH:mm}");
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có vấn đề về Database
                return (false, "Lỗi hệ thống khi cập nhật VIP: " + ex.Message);
            }
        }
        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            // Gọi xuống Repo để lấy dữ liệu từ SQL Server
            var transactions = await _repository.GetAllTransactionsAsync();

            // Bạn có thể thêm logic lọc hoặc sắp xếp thêm ở đây nếu cần
            return transactions ?? new List<Transaction>();
        }
        public async Task<IEnumerable<ReporterRankingDto>> GetTopReportersAsync(int count)
        {
            return await _repository.GetTopReportersAsync(count);
        }
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            // Gọi xuống Repo để lấy dữ liệu thực tế từ SQL Server
            var users = await _repository.GetAllUsersAsync();

            // Nếu null, trả về danh sách rỗng để tránh lỗi NullReferenceException ở View
            return users ?? new List<User>();
        }


    }
}
