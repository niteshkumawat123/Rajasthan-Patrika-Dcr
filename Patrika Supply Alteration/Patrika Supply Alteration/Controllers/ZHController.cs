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
        var roles = user.RoleDetails ?? new List<RoleDetails>();
        bool canAdd = roles.Any(r => r.RoleId == "1" || r.RoleId == "3" || r.RoleId == "4" || r.RoleId == "6");
        bool canApprove = roles.Any(r => r.RoleId == "4");

        var stats = await _dbService.GetZHStatsAsync(user.EmpCode!, user.ComCode!);
        var pending = await _dbService.GetZHPendingAsync(user.EmpCode!, user.ComCode!);
        var model = new ZHDashboardViewModel
        {
            AwaitingMe = stats.awaitingMe,
            AtHo = stats.atHo,
            Approved = stats.approved,
            Rejected = stats.rejected,
            PendingRequests = pending,
            User = user,
            CanAddRequest = canAdd,
            CanApprove = canApprove
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult NewRequest()
    {
        return View("~/Views/Home/NewRequest.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> AgentLookup(string agcd)
    {
        var user = GetUser();
        var agent = await _dbService.GetAgentAsync(agcd, user.ComCode!);
        if (agent == null)
            return Json(new { found = false });
        var supplies = await _dbService.GetSupplyAsync(agent.Agcd!, agent.Dpcd!, user.ComCode!);
        return Json(new { found = true, agent, supplies });
    }

    [HttpGet]
    public async Task<IActionResult> AgentSearch(string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(new List<object>());
        var user = GetUser();
        var branchCodes = user.BranchDetails?.Select(b => b.BranchCode).Where(b => b != null).ToList();
        var results = await _dbService.SearchAgentsByBranchAsync(q, user.ComCode!, branchCodes);
        return Json(results);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitRequest([FromBody] SupplyRequestViewModel model)
    {
        var user = GetUser();
        model.UserId = user.UserId;
        model.CompCode = user.ComCode;
        model.UnitCode = user.UnitCode;
        model.ZoneCode = user.Zone;

        var hasPending = await _dbService.HasPendingRequestAsync(model.Agcd!, model.Dpcd!, user.ComCode!);
        if (hasPending)
            return Json(new { success = false, message = "A pending request already exists for this agency." });

        var success = await _dbService.SubmitRequestAsync(model);
        return Json(new { success, message = success ? "Request submitted successfully!" : "Failed to submit request." });
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var user = GetUser();
        var list = await _dbService.GetSEHistoryAsync(user.UserId!, user.ComCode!);
        return View("~/Views/Home/History.cshtml", list);
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
