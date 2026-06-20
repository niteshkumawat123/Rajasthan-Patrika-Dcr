using MailKit.Net.Smtp;
using MimeKit;

namespace DCRSupplyApp.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendForgotPasswordEmailAsync(string toEmail, string employeeId, string password)
    {
        var smtp = _config.GetSection("SMTP");

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtp["DisplayName"], smtp["Email"]));
            message.To.Add(new MailboxAddress(employeeId, toEmail));
            message.Subject = "DCR Supply App — Your Password";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"<p>Dear Employee <b>{employeeId}</b>,</p>
                    <p>Your password for DCR Supply App is: <b>{password}</b></p>
                    <p>Please keep this confidential.</p>
                    <p>— Rajasthan Patrika</p>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var port = int.Parse(smtp["PortNo"]!);
            var host = smtp["Host"]!;

            _logger.LogInformation("Connecting to SMTP: {Host}:{Port}", host, port);
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.Auto);
            _logger.LogInformation("SMTP connected. IsConnected={Connected}, IsSecure={Secure}", client.IsConnected, client.IsSecure);

            _logger.LogInformation("Authenticating as {Email}", smtp["Email"]);
            await client.AuthenticateAsync(smtp["Email"], smtp["Password"]);
            _logger.LogInformation("Authenticated. IsAuthenticated={Auth}", client.IsAuthenticated);

            _logger.LogInformation("Sending email to {To}", toEmail);
            var response = await client.SendAsync(message);
            _logger.LogInformation("SMTP server response: {Response}", response);

            await client.DisconnectAsync(true);
            _logger.LogInformation("Forgot password email sent successfully for employee {EmployeeId}", employeeId);
        }
        catch (MailKit.Security.AuthenticationException authEx)
        {
            _logger.LogError(authEx, "SMTP Authentication failed for {Email} on {Host}", smtp["Email"], smtp["Host"]);
            throw;
        }
        catch (MailKit.Net.Smtp.SmtpCommandException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP command error. StatusCode={StatusCode}, ErrorCode={ErrorCode}, Message={Msg}",
                smtpEx.StatusCode, smtpEx.ErrorCode, smtpEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send forgot password email. ExceptionType={Type}, Message={Msg}",
                ex.GetType().FullName, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Send a notification email to a list of recipients (like push notification but via email)
    /// </summary>
    public async Task SendNotificationEmailAsync(List<string> toEmails, string subject, string body)
    {
        if (toEmails == null || toEmails.Count == 0) return;

        var smtp = _config.GetSection("SMTP");
        var validEmails = toEmails.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
        if (validEmails.Count == 0) return;

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtp["DisplayName"], smtp["Email"]));

            foreach (var email in validEmails)
            {
                message.To.Add(new MailboxAddress(email, email));
            }

            message.Subject = $"DCR Supply App — {subject}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = BuildNotificationHtml(subject, body)
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtp["Host"], int.Parse(smtp["PortNo"]!), MailKit.Security.SecureSocketOptions.Auto);
            await client.AuthenticateAsync(smtp["Email"], smtp["Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Notification email sent to {Count} recipients. Subject: {Subject}", validEmails.Count, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification email. Subject: {Subject}", subject);
        }
    }

    /// <summary>
    /// Send notification email to a single recipient
    /// </summary>
    public async Task SendNotificationEmailAsync(string toEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(toEmail)) return;
        await SendNotificationEmailAsync(new List<string> { toEmail }, subject, body);
    }

    private static string BuildNotificationHtml(string title, string body)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin:0;padding:0;background:#f4f6f8;font-family:Arial,sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f4f6f8;padding:30px 0;"">
        <tr>
            <td align=""center"">
                <table width=""500"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,0.08);overflow:hidden;"">
                    <tr>
                        <td style=""background:linear-gradient(135deg,#1a237e,#283593);padding:24px 30px;"">
                            <h2 style=""color:#ffffff;margin:0;font-size:18px;"">&#x1F514; {title}</h2>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:28px 30px;"">
                            <p style=""color:#333;font-size:15px;line-height:1.6;margin:0 0 20px 0;"">{body}</p>
                            <hr style=""border:none;border-top:1px solid #eee;margin:20px 0;"">
                            <p style=""color:#888;font-size:12px;margin:0;"">This is an automated notification from DCR Supply Alteration System.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background:#f8f9fa;padding:16px 30px;text-align:center;"">
                            <p style=""color:#999;font-size:11px;margin:0;"">Rajasthan Patrika - Supply Alteration App</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
