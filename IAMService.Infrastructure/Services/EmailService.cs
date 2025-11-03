using IAMService.Application.Interfaces;
using IAMService.Infrastructure.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace IAMService.Infrastructure.Services
{
    /// <summary>
    /// Implementation of email service
    /// Currently logs emails to console - integrate with actual email provider (SendGrid, SMTP, etc.) in production
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _settings;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger;
        }

        /// <summary>
        /// Sends welcome email with auto-generated password to new patient
        /// </summary>
        /// <param name="toEmail">Patient's email address</param>
        /// <param name="fullName">Patient's full name</param>
        /// <param name="generatedPassword">Auto-generated password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SendNewPatientAccountEmailAsync(
            string toEmail,
            string fullName,
            string generatedPassword,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending new patient account email to {Email}", toEmail);
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.From));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = "🏥 Chào mừng bạn đến với Hệ thống Chăm sóc Sức khỏe";
                var builder = new BodyBuilder();
                builder.HtmlBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
        <h2 style='color: #0066cc; text-align: center;'>🏥 Chào mừng đến với IAM Healthcare</h2>
        
        <p>Xin chào <strong>{fullName}</strong>,</p>

        <p>
            Chúng tôi rất vui mừng thông báo rằng tài khoản bệnh nhân của bạn đã được tạo thành công! 
            Bạn có thể sử dụng thông tin đăng nhập dưới đây để truy cập hệ thống.
        </p>

        <div style='background: #f9f9f9; padding: 15px; border-left: 4px solid #0066cc; margin: 20px 0;'>
            <h3 style='margin-top: 0;'>🔐 Thông tin đăng nhập:</h3>
            <p style='margin: 5px 0;'><strong>Email:</strong> {toEmail}</p>
            <p style='margin: 5px 0;'><strong>Mật khẩu tạm thời:</strong> <code style='background: #fff; padding: 4px 8px; border-radius: 4px; font-size: 14px;'>{generatedPassword}</code></p>
        </div>

        <div style='background: #fff3cd; padding: 12px; border-left: 4px solid #ffc107; margin: 20px 0;'>
            <p style='margin: 0;'>
                <strong>⚠️ Lưu ý bảo mật:</strong><br>
                Vì lý do bảo mật, vui lòng <strong>đổi mật khẩu ngay sau lần đăng nhập đầu tiên</strong>.
            </p>
        </div>

        <h3>📋 Các bước tiếp theo:</h3>
        <ol>
            <li>Truy cập hệ thống tại: <a href='https://healthcare-system.com' style='color: #0066cc;'>https://healthcare-system.com</a></li>
            <li>Đăng nhập bằng email và mật khẩu tạm thời ở trên</li>
            <li>Đổi mật khẩu trong phần Cài đặt tài khoản</li>
            <li>Cập nhật thông tin cá nhân của bạn</li>
        </ol>

        <p style='margin-top: 30px;'>
            Nếu bạn <strong>không yêu cầu tạo tài khoản này</strong>, vui lòng liên hệ với chúng tôi ngay lập tức qua email 
            <a href='mailto:{_settings.From}' style='color: #0066cc;'>{_settings.From}</a>.
        </p>

        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>

        <p style='color: #666; font-size: 13px; text-align: center;'>
            Trân trọng,<br>
            <strong>{_settings.FromName}</strong><br>
            Email: {_settings.From}
        </p>
    </div>
</body>
</html>";

                message.Body = builder.ToMessageBody();
                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _settings.Host,
                    _settings.Port,
                    MailKit.Security.SecureSocketOptions.StartTls,
                    cancellationToken);
                await client.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password,
                    cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Patient account email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send patient account email to {Email}", toEmail);
                throw;
            }
        }

        /// <summary>
        /// Sends welcome email to new employee
        /// </summary>
        /// <param name="toEmail">Employee's email address</param>
        /// <param name="fullName">Employee's full name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SendNewEmployeeAccountEmailAsync(
           string toEmail,
           string fullName,
           CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending new employee account email to {Email}", toEmail);

                // ========== Tạo MimeMessage ==========
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.From));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = "🎉 Chào mừng bạn gia nhập đội ngũ!";

                // ========== Tạo HTML Body ==========
                var builder = new BodyBuilder();
                builder.HtmlBody = $@"
<html>
<body style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
        <h2 style='color: #28a745; text-align: center;'>🎉 Chào mừng bạn gia nhập đội ngũ!</h2>
        
        <p>Xin chào <strong>{fullName}</strong>,</p>

        <p>
            Chúng tôi rất vui mừng chào đón bạn gia nhập đội ngũ nhân viên của <strong>{_settings.FromName}</strong>! 
            Tài khoản nhân viên của bạn đã được tạo thành công.
        </p>

        <div style='background: #d1ecf1; padding: 15px; border-left: 4px solid #17a2b8; margin: 20px 0;'>
            <h3 style='margin-top: 0; color: #0c5460;'>📧 Thông tin tài khoản:</h3>
            <p style='margin: 5px 0; color: #0c5460;'><strong>Email:</strong> {toEmail}</p>
            <p style='margin: 5px 0; color: #0c5460;'><strong>Trạng thái:</strong> Đang chờ xác minh</p>
        </div>

        <div style='background: #fff3cd; padding: 12px; border-left: 4px solid #ffc107; margin: 20px 0;'>
            <p style='margin: 0;'>
                <strong>⏳ Vui lòng đợi xác minh:</strong><br>
                Tài khoản của bạn hiện đang được quản trị viên xác minh. 
                Bạn sẽ nhận được email thông báo khi tài khoản được kích hoạt.
            </p>
        </div>

        <h3>📋 Các bước tiếp theo:</h3>
        <ol>
            <li>Chờ email xác nhận kích hoạt tài khoản</li>
            <li>Sau khi nhận được email, truy cập hệ thống để đăng nhập</li>
            <li>Hoàn thành hồ sơ nhân viên của bạn</li>
            <li>Tham gia các khóa đào tạo định hướng</li>
        </ol>

        <p style='margin-top: 30px;'>
            Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với bộ phận Nhân sự qua email 
            <a href='mailto:{_settings.From}' style='color: #0066cc;'>{_settings.From}</a>.
        </p>

        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>

        <p style='color: #666; font-size: 13px; text-align: center;'>
            Trân trọng,<br>
            <strong>Bộ phận Nhân sự</strong><br>
            {_settings.FromName}<br>
            Email: {_settings.From}
        </p>
    </div>
</body>
</html>";

                message.Body = builder.ToMessageBody();
                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _settings.Host,
                    _settings.Port,
                    MailKit.Security.SecureSocketOptions.StartTls,
                    cancellationToken);

                await client.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password,
                    cancellationToken);

                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Employee account email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send employee account email to {Email}", toEmail);
                throw;
            }
        }
        public async Task SendPasswordResetEmailAsync(string toEmail, string subject, string callbackUrl, CancellationToken cancellationToken = default)
        {
            // Cần phải bọc toàn bộ logic trong một khối try-catch để log lỗi gửi mail
            try
            {
                _logger.LogInformation("Attempting to send password reset email to {Email}", toEmail);


                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.From));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;


                var builder = new BodyBuilder();
                builder.HtmlBody = $@"
<html>
<body style='font-family: Arial, sans-serif; color: #333; line-height: 1.6; background-color: #f4f4f4; margin: 0; padding: 20px;'>
    <div style='max-width: 600px; margin: 0 auto; background: #ffffff; padding: 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow: hidden;'>
        
        <!-- Header with gradient -->
        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;'>🔐 Đặt lại Mật khẩu</h1>
            <p style='color: #f0f0f0; margin: 10px 0 0 0; font-size: 14px;'>Password Reset Request</p>
        </div>

        <!-- Content -->
        <div style='padding: 40px 30px;'>
            <p style='font-size: 16px; margin: 0 0 20px 0;'>Xin chào,</p>
            
            <p style='font-size: 15px; margin: 0 0 25px 0; color: #555;'>
                Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. 
                Để tiếp tục, vui lòng nhấp vào nút bên dưới:
            </p>

            <!-- CTA Button -->
            <div style='text-align: center; margin: 35px 0;'>
                <a href='{callbackUrl}' 
                   style='display: inline-block; padding: 16px 40px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                          color: #ffffff; text-decoration: none; border-radius: 50px; font-weight: 600; 
                          font-size: 16px; box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4); 
                          transition: all 0.3s ease;'>
                    ✨ ĐẶT LẠI MẬT KHẨU
                </a>
            </div>

            <!-- Alternative link -->
            <p style='font-size: 13px; color: #888; text-align: center; margin: 20px 0;'>
                Hoặc copy link sau vào trình duyệt:<br>
                <a href='{callbackUrl}' style='color: #667eea; word-break: break-all; font-size: 12px;'>{callbackUrl}</a>
            </p>

            <!-- Warning box -->
            <div style='background: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 30px 0; border-radius: 4px;'>
                <p style='margin: 0; font-size: 14px; color: #856404;'>
                    <strong>⏱️ Lưu ý quan trọng:</strong><br>
                    Liên kết này chỉ có hiệu lực trong <strong>vòng 15 phút</strong> kể từ khi email được gửi. 
                    Sau thời gian này, bạn sẽ cần yêu cầu đặt lại mật khẩu mới.
                </p>
            </div>

            <!-- Security notice -->
            <div style='background: #f8d7da; padding: 15px; border-left: 4px solid #dc3545; margin: 20px 0; border-radius: 4px;'>
                <p style='margin: 0; font-size: 14px; color: #721c24;'>
                    <strong>🔒 Bảo mật:</strong><br>
                    Nếu bạn <strong>không yêu cầu</strong> thay đổi mật khẩu, vui lòng bỏ qua email này và 
                    liên hệ với chúng tôi ngay lập tức để đảm bảo an toàn cho tài khoản.
                </p>
            </div>

            <!-- Tips -->
            <div style='background: #d1ecf1; padding: 15px; border-left: 4px solid #17a2b8; margin: 20px 0; border-radius: 4px;'>
                <h3 style='margin: 0 0 10px 0; font-size: 15px; color: #0c5460;'>💡 Gợi ý tạo mật khẩu mạnh:</h3>
                <ul style='margin: 0; padding-left: 20px; font-size: 13px; color: #0c5460;'>
                    <li>Sử dụng ít nhất 8 ký tự</li>
                    <li>Kết hợp chữ hoa, chữ thường, số và ký tự đặc biệt</li>
                    <li>Không sử dụng thông tin cá nhân dễ đoán</li>
                    <li>Không sử dụng lại mật khẩu cũ</li>
                </ul>
            </div>

        </div>

        <!-- Footer -->
        <div style='background: #f8f9fa; padding: 25px 30px; border-top: 1px solid #e9ecef;'>
            <p style='margin: 0 0 10px 0; font-size: 14px; color: #666;'>
                Trân trọng,<br>
                <strong style='color: #333;'>{_settings.FromName}</strong>
            </p>
            <p style='margin: 15px 0 0 0; font-size: 12px; color: #999;'>
                📧 Email: <a href='mailto:{_settings.From}' style='color: #667eea; text-decoration: none;'>{_settings.From}</a>
            </p>
            
            <hr style='border: none; border-top: 1px solid #e9ecef; margin: 20px 0;'>
            
            <p style='margin: 0; font-size: 11px; color: #aaa; text-align: center;'>
                Email này được gửi tự động. Vui lòng không trả lời email này.<br>
                © 2025 {_settings.FromName}. All rights reserved.
            </p>
        </div>

    </div>
</body>
</html>";
                message.Body = builder.ToMessageBody();
                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _settings.Host,
                    _settings.Port,
                    MailKit.Security.SecureSocketOptions.StartTls,
                    cancellationToken);
                await client.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password,
                    cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);
                _logger.LogInformation("Password reset email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
                throw new InvalidOperationException($"Không thể gửi email khôi phục mật khẩu đến {toEmail}. Lỗi: {ex.Message}", ex);
            }
        }
    }
}