using DCRSupplyApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DCRSupplyApp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly FirebaseNotificationService _fcm;

    public NotificationController(FirebaseNotificationService fcm)
    {
        _fcm = fcm;
    }

    [HttpPost("register-token")]
    public IActionResult RegisterToken([FromBody] RegisterTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.EmpCode) || string.IsNullOrEmpty(request.Token))
            return BadRequest();

        _fcm.RegisterToken(request.EmpCode, request.Token);

        // Subscribe to role-based topic
        if (!string.IsNullOrEmpty(request.RoleTopic))
        {
            _ = _fcm.SubscribeToTopicAsync(request.Token, request.RoleTopic);
        }

        return Ok(new { success = true });
    }

    [HttpPost("remove-token")]
    public IActionResult RemoveToken([FromBody] RegisterTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.EmpCode) || string.IsNullOrEmpty(request.Token))
            return BadRequest();

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
