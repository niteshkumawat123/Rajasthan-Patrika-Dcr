using MailKit.Net.Smtp;
using MimeKit;

namespace DCRSupplyApp.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendForgotPasswordEmailAsync(string toEmail, string employeeId, string password)
    {
        var smtp = _config.GetSection("SMTP");
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtp["DisplayName"], smtp["Email"]));
        message.To.Add(new MailboxAddress(employeeId, toEmail));
        message.Subject = "DCR Supply App — Your Password";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"<p>Dear Employee <b>{employeeId}</b>,</p>
                <p>Your password for DCR Supply App is: <b>{password}</b></p>
                <p>Please keep this confidential.</p>
                <p>— Vertex Plus Team</p>"
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(smtp["Host"], int.Parse(smtp["PortNo"]!), MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(smtp["Email"], smtp["Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
