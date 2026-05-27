using Microsoft.AspNetCore.Mvc;
using DCRSupplyApp.Models;
using DCRSupplyApp.Services;
using DCRSupplyApp.Filters;

namespace DCRSupplyApp.Controllers;

[ServiceFilter(typeof(SessionAuthFilter))]
public class ZHController : Controller
{
    private readonly OracleDbService _dbService;
    private readonly SessionService _sessionService;

    public ZHController(OracleDbService dbService, SessionService sessionService)
    {
        _dbService = dbService;
        _sessionService = sessionService;
    }

    private UserSessionModel GetUser() => _sessionService.GetUser(HttpContext.Session)!;

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var user = GetUser();
        var stats = await _dbService.GetZHStatsAsync(user.BranchCode!, user.ComCode!);
        var pending = await _dbService.GetZHPendingAsync(user.BranchCode!, user.ComCode!);
        var model = new ZHDashboardViewModel
        {
            AwaitingMe = stats.awaitingMe,
            AtHo = stats.atHo,
            Approved = stats.approved,
            Rejected = stats.rejected,
            PendingRequests = pending,
            User = user
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(decimal reqId, string remarks)
    {
        var user = GetUser();
        var success = await _dbService.ZHApproveRejectAsync(reqId, "APPROVED", user.UserId!, remarks ?? "", user.ComCode!);
        return Json(new { success, message = success ? "Request approved." : "Failed to approve." });
    }

    [HttpPost]
    public async Task<IActionResult> Reject(decimal reqId, string remarks)
    {
        var user = GetUser();
        var success = await _dbService.ZHApproveRejectAsync(reqId, "REJECTED", user.UserId!, remarks ?? "", user.ComCode!);
        return Json(new { success, message = success ? "Request rejected." : "Failed to reject." });
    }

    [HttpGet]
    public async Task<IActionResult> AuditTrail(decimal reqId)
    {
        var trail = await _dbService.GetAuditTrailAsync(reqId);
        return Json(trail);
    }
}
