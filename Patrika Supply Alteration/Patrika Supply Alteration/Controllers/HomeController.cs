using Microsoft.AspNetCore.Mvc;
using DCRSupplyApp.Models;
using DCRSupplyApp.Services;
using DCRSupplyApp.Filters;

namespace DCRSupplyApp.Controllers;

[ServiceFilter(typeof(SessionAuthFilter))]
public class HomeController : Controller
{
    private readonly OracleDbService _dbService;
    private readonly SessionService _sessionService;
    private readonly NotifyService _notifyService;

    public HomeController(OracleDbService dbService, SessionService sessionService, NotifyService notifyService)
    {
        _dbService = dbService;
        _sessionService = sessionService;
        _notifyService = notifyService;
    }

    private UserSessionModel GetUser() => _sessionService.GetUser(HttpContext.Session)!;

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var user = GetUser();
        var roles = user.RoleDetails ?? new List<RoleDetails>();
        bool canAdd = roles.Any(r => r.RoleId == "1" || r.RoleId == "3" || r.RoleId == "4" || r.RoleId == "6");
        bool canApprove = roles.Any(r => r.RoleId == "4" || r.RoleId == "7");

        var stats = await _dbService.GetSEStatsAsync(user.EmpCode!, user.ComCode!);
        var recent = await _dbService.GetSERecentRequestsAsync(user.EmpCode!, user.ComCode!);
        var model = new DashboardViewModel
        {
            Pending = stats.pending,
            Approved = stats.approved,
            Rejected = stats.rejected,
            Today = stats.today,
            RecentRequests = recent,
            User = user,
            CanAddRequest = canAdd,
            CanApprove = canApprove
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult NewRequest()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> AgentLookup(string agcd,string dpcd)
    {
        var user = GetUser();
        var agent = await _dbService.GetAgentAsync(agcd, user.ComCode!);
        if (agent == null)
        {
            return Json(new { found = false });
        }
        agent.Dpcd = dpcd;
        var supplies = await _dbService.GetSupplyAsync(agent.Agcd!, dpcd!, user.ComCode!);
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
        model.EmployeeCode = user.EmpCode;

        // Check if there is already a pending request for this agency
        var hasPending = await _dbService.HasPendingRequestAsync(model.Agcd!, model.Dpcd!, user.ComCode!);
        if (hasPending)
            return Json(new { success = false, message = "A pending request already exists for this agency. Please wait for approval or cancellation before submitting a new one." });

        var success = await _dbService.SubmitRequestAsync(model);
        if (success)
        {
            _ = Task.Run(async () =>
            {
                var branchCode = model.UnitCode;
                if (!string.IsNullOrEmpty(branchCode))
                    await _notifyService.NotifyZHByBranchAsync(branchCode, "New Supply Request", $"A new supply alteration request has been submitted by {user.EmpName} ({user.EmpCode}) for agent {model.Agcd}. Please review and take action.");
            });
        }
        return Json(new { success, message = success ? "Request submitted successfully! Redirecting..." : "Failed to submit request." });
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var user = GetUser();
        var list = await _dbService.GetSEHistoryAsync(user.EmpCode!, user.ComCode!);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> AuditTrail(decimal reqId)
    {
        var trail = await _dbService.GetAuditTrailAsync(reqId);
        return Json(trail);
    }
}
