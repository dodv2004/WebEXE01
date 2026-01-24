using System.Net;
using System.Net.Mail;

public class EmailService
{
    public async Task SendOTPAsync(string toEmail, string otp)
    {
        var fromEmail = "dinhvando2k5@gmail.com";
        var password = "hfcf zzmx sesn gokw"; // Password ứng dụng (App Password)

        var message = new MailMessage(fromEmail, toEmail);
        message.Subject = "Mã xác thực OTP - AntiScam";
        message.Body = $"Mã OTP của bạn là: {otp}. Mã này có hiệu lực trong 5 phút.";

        using var client = new SmtpClient("smtp.gmail.com", 587);
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential(fromEmail, password);
        await client.SendMailAsync(message);
    }
}