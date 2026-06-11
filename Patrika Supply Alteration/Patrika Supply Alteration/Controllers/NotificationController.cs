using DCRSupplyApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DCRSupplyApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly FirebaseNotificationService _fcm;
    private readonly OracleDbService _dbService;

    public NotificationController(FirebaseNotificationService fcm, OracleDbService dbService)
    {
        _fcm = fcm;
        _dbService = dbService;
    }

    [HttpPost("register-token")]
    public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.EmpCode) || string.IsNullOrEmpty(request.Token))
            return BadRequest();

        // Save token to DB (persistent)
        await _dbService.SavePushTokenAsync(request.EmpCode, request.Token);

        // Also keep in-memory for topic subscription
        _fcm.RegisterToken(request.EmpCode, request.Token);

        // Subscribe to role-based topic (kept for backward compatibility)
        if (!string.IsNullOrEmpty(request.RoleTopic))
        {
            _ = _fcm.SubscribeToTopicAsync(request.Token, request.RoleTopic);
        }

        return Ok(new { success = true });
    }

    [HttpPost("remove-token")]
    public async Task<IActionResult> RemoveToken([FromBody] RegisterTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.EmpCode) || string.IsNullOrEmpty(request.Token))
            return BadRequest();

        // Clear from DB
        await _dbService.ClearPushTokenAsync(request.EmpCode);

        // Clear from memory
        _fcm.RemoveToken(request.EmpCode, request.Token);

        return Ok(new { success = true });
    }
}

public class RegisterTokenRequest
{
    public string? EmpCode { get; set; }
    public string? Token { get; set; }
    public string? RoleTopic { get; set; }
}
