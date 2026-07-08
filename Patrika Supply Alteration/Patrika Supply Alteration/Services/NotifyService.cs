using DCRSupplyApp.Models;

namespace DCRSupplyApp.Services;

/// <summary>
/// Unified notification service — sends both push (FCM) and email notifications.
/// Uses tokens stored in LOGIN table (DB-persistent, not in-memory).
/// </summary>
public class NotifyService
{
    private readonly OracleDbService _dbService;
    private readonly FirebaseNotificationService _firebase;
    private readonly EmailService _emailService;
    private readonly ILogger<NotifyService> _logger;

    public NotifyService(OracleDbService dbService, FirebaseNotificationService firebase, EmailService emailService, ILogger<NotifyService> logger)
    {
        _dbService = dbService;
        _firebase = firebase;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Notify all ZH users of a specific branch (push + email)
    /// </summary>
    public async Task NotifyZHByBranchAsync(string branchCode, string title, string body)
    {
        try
        {
            // Get ZH push tokens from DB
            var tokens = await _dbService.GetPushTokensByRoleAndBranchAsync("4", branchCode);
            await SendPushToTokensAsync(tokens, title, body);

            // Get ZH emails
            var emails = await _dbService.GetZHEmailsByBranchAsync(branchCode);
            if (emails.Count > 0)
                await _emailService.SendNotificationEmailAsync(emails, title, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying ZH for branch {Branch}", branchCode);
        }
    }

    /// <summary>
    /// Notify HO users of a specific branch (push + email)
    /// </summary>
    public async Task NotifyAllHOAsync(string branchCode, string title, string body)
    {
        try
        {
            // Get HO push tokens filtered by branch from DB
            var tokens = await _dbService.GetPushTokensByRoleAndBranchAsync("7", branchCode);
            await SendPushToTokensAsync(tokens, title, body);

            // Get HO emails filtered by branch
            var emails = await _dbService.GetHOEmailsByBranchAsync(branchCode);
            if (emails.Count > 0)
                await _emailService.SendNotificationEmailAsync(emails, title, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying HO users for branch {Branch}", branchCode);
        }
    }

    /// <summary>
    /// Notify a specific employee by EmpCode (push + email)
    /// </summary>
    public async Task NotifyEmployeeAsync(string empCode, string title, string body)
    {
        try
        {
            // Get push token from DB
            var token = await _dbService.GetPushTokenByEmpCodeAsync(empCode);
            if (!string.IsNullOrEmpty(token))
                await SendPushToTokensAsync(new List<string> { token }, title, body);

            // Get email
            var email = await _dbService.GetEmployeeEmailAsync(empCode);
            if (!string.IsNullOrEmpty(email))
                await _emailService.SendNotificationEmailAsync(email, title, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying employee {EmpCode}", empCode);
        }
    }

    /// <summary>
    /// Notify the creator of a request (push + email)
    /// </summary>
    public async Task NotifyRequestCreatorAsync(decimal reqId, string title, string body)
    {
        try
        {
            var creatorEmpCode = await _dbService.GetRequestCreatorAsync(reqId);
            if (!string.IsNullOrEmpty(creatorEmpCode))
                await NotifyEmployeeAsync(creatorEmpCode, title, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying request creator for ReqId {ReqId}", reqId);
        }
    }

    /// <summary>
    /// Send FCM push notification directly to a list of tokens.
    /// Uses data-only messages so service worker always fires (even when browser is closed).
    /// </summary>
    private async Task SendPushToTokensAsync(List<string> tokens, string title, string body)
    {
        if (tokens == null || tokens.Count == 0) return;

        var validTokens = tokens.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
        if (validTokens.Count == 0) return;

        try
        {
            foreach (var token in validTokens)
            {
                try
                {
                    var message = new FirebaseAdmin.Messaging.Message
                    {
                        Token = token,
                        Data = new Dictionary<string, string>
                        {
                            { "title", title },
                            { "body", body },
                            { "icon", "/icons/PatrikaFaveIcon.jpeg" },
                            { "timestamp", DateTime.Now.Ticks.ToString() }
                        },
                        Webpush = new FirebaseAdmin.Messaging.WebpushConfig
                        {
                            Headers = new Dictionary<string, string>
                            {
                                { "Urgency", "high" },
                                { "TTL", "86400" }
                            }
                        }
                    };
                    await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(message);
                }
                catch (FirebaseAdmin.Messaging.FirebaseMessagingException fex)
                {
                    _logger.LogWarning("Push failed for token {Token}: {Error}", token[..20], fex.Message);
                }
            }
            _logger.LogInformation("Push sent to {Count} tokens. Title: {Title}", validTokens.Count, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notifications");
        }
    }
}
