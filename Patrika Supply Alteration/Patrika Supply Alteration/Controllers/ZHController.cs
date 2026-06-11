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
    private readonly NotifyService _notifyService;

    public ZHController(OracleDbService dbService, SessionService sessionService, NotifyService notifyService)
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
        bool canApprove = roles.Any(r => r.RoleId == "4");
        var branchCodes = user.BranchDetails?.Select(b => b.BranchCode).Where(b => b != null).ToList();

        var stats = await _dbService.GetZHStatsAsync(user.EmpCode!, user.ComCode!, branchCodes);
        var pending = await _dbService.GetZHPendingAsync(user.EmpCode!, user.ComCode!, branchCodes);
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
    public async Task<IActionResult> AgentLookup(string agcd, string dpcd)
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
        model.UnitCode = user.BranchDetails.Select(x=>x.BranchCode).FirstOrDefault();
        model.ZoneCode = user.Zone;
        model.EmployeeCode = user.EmpCode;

        var hasPending = await _dbService.HasPendingRequestAsync(model.Agcd!, model.Dpcd!, user.ComCode!);
        if (hasPending)
            return Json(new { success = false, message = "A pending request already exists for this agency." });

        var success = await _dbService.SubmitRequestAsync(model);
        if (success)
        {
            _ = Task.Run(async () =>
            {
                var branchCode = model.UnitCode;
                if (!string.IsNullOrEmpty(branchCode))
                    await _notifyService.NotifyZHByBranchAsync(branchCode, "New Supply Request", $"A new supply alteration request has been submitted by {user.EmpName} for agent {model.Agcd}. Please review and take action.");
            });
        }
        return Json(new { success, message = success ? "Request submitted successfully!" : "Failed to submit request." });
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var user = GetUser();
        var list = await _dbService.GetSEHistoryAsync(user.EmpCode!, user.ComCode!);
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(decimal reqId, string remarks)
    {
        var user = GetUser();
        var success = await _dbService.ZHApproveRejectAsync(reqId, "APPROVED", user.EmpCode!, remarks ?? "", user.ComCode!);
        if (success)
        {
            _ = Task.Run(async () =>
            {
                await _notifyService.NotifyAllHOAsync("Request Approved by ZH", $"Request #{reqId} has been approved by ZH ({user.EmpName}) and forwarded to HO for final approval.");
                await _notifyService.NotifyRequestCreatorAsync(reqId, "Request Forwarded to HO", $"Your request #{reqId} has been approved by ZH and forwarded to HO for final approval.");
            });
        }
        return Json(new { success, message = success ? "Request approved." : "Failed to approve." });
    }

    [HttpPost]
    public async Task<IActionResult> Reject(decimal reqId, string remarks)
    {
        var user = GetUser();
        var success = await _dbService.ZHApproveRejectAsync(reqId, "REJECTED", user.EmpCode!, remarks ?? "", user.ComCode!);
        if (success)
        {
            _ = Task.Run(async () =>
            {
                var remarksText = !string.IsNullOrEmpty(remarks) ? $" Remarks: {remarks}" : "";
                await _notifyService.NotifyRequestCreatorAsync(reqId, "Request Rejected by ZH", $"Your request #{reqId} has been rejected by ZH ({user.EmpName}).{remarksText}");
            });
        }
        return Json(new { success, message = success ? "Request rejected." : "Failed to reject." });
    }

    [HttpGet]
    public async Task<IActionResult> AuditTrail(decimal reqId)
    {
        var trail = await _dbService.GetAuditTrailAsync(reqId);
        return Json(trail);
    }

    [HttpGet]
    public async Task<IActionResult> FilteredRequests(string status)
    {
        var user = GetUser();
        var branchCodes = user.BranchDetails?.Select(b => b.BranchCode).Where(b => b != null).ToList();
        List<SupplyRequestViewModel> list;
        switch (status)
        {
            case "awaiting":
                list = await _dbService.GetZHPendingAsync(user.EmpCode!, user.ComCode!, branchCodes);
                break;
            case "approved":
                list = await _dbService.GetZHApprovedByMeAsync(user.EmpCode!, user.ComCode!, branchCodes);
                break;
            case "atho":
                list = await _dbService.GetZHAtHoAsync(user.EmpCode!, user.ComCode!, branchCodes);
                break;
            case "rejected":
                list = await _dbService.GetZHRejectedAsync(user.EmpCode!, user.ComCode!, branchCodes);
                break;
            default:
                list = await _dbService.GetZHPendingAsync(user.EmpCode!, user.ComCode!, branchCodes);
                break;
        }
        return Json(list);
    }
}
