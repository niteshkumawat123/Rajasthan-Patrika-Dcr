using Microsoft.AspNetCore.Mvc;
using DCRSupplyApp.Models;
using DCRSupplyApp.Services;
using DCRSupplyApp.Filters;

namespace DCRSupplyApp.Controllers;

[ServiceFilter(typeof(SessionAuthFilter))]
public class HOController : Controller
{
    private readonly OracleDbService _dbService;
    private readonly SessionService _sessionService;
    private readonly NotifyService _notifyService;

    public HOController(OracleDbService dbService, SessionService sessionService, NotifyService notifyService)
    {
        _dbService = dbService;
        _sessionService = sessionService;
        _notifyService = notifyService;
    }

    private UserSessionModel GetUser() => _sessionService.GetUser(HttpContext.Session)!;

    private async Task<List<string?>> GetHOBranchCodesAsync()
    {
        var user = GetUser();
        var branches = await _dbService.GetHOAllowedBranchesAsync(user.EmpCode!);
        return branches.Cast<string?>().ToList();
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(DateTime? selectedDate)
    {
        var user = GetUser();
        var date = selectedDate ?? DateTime.Today;
        var branchCodes = await GetHOBranchCodesAsync();

        var stats = await _dbService.GetHOStatsAsync(user.ComCode!, date, branchCodes, user.EmpCode!);
        var pending = await _dbService.GetHOPendingAsync(user.ComCode!, branchCodes);
        var model = new HODashboardViewModel
        {
            AwaitingHo = stats.awaitingHo,
            HoApproved = stats.hoApproved,
            TotalIncreased = stats.totalIncreased,
            TotalDecreased = stats.totalDecreased,
            HoRejected = stats.hoRejected,
            SelectedDate = date,
            PendingRequests = pending,
            User = user
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(decimal reqId, string remarks, string agcd, string dpcd,
        string publ, string edtn, string supplyTypeCode, decimal changedSupply, DateTime? changedSupplyDate, string unitCode)
    {
        var user = GetUser();
        if (!await _dbService.IsRequestCreatedTodayAsync(reqId))
            return Json(new { success = false, message = "Only today's requests can be approved. This request was not created today." });
        var success = await _dbService.HOApproveAsync(reqId, user.EmpCode!, remarks ?? "", user.ComCode!,
            unitCode, agcd, dpcd, publ, edtn, supplyTypeCode, changedSupply, changedSupplyDate);
        if (success)
        {
            _ = Task.Run(async () =>
            {
                var branch = await _dbService.GetRequestBranchAsync(reqId);
                if (!string.IsNullOrEmpty(branch))
                    await _notifyService.NotifyZHByBranchAsync(branch, "HO Approved", $"Request #{reqId} has been approved by HO ({user.EmpName}) and pushed to ERP.");
                await _notifyService.NotifyRequestCreatorAsync(reqId, "Request Approved", $"Your request #{reqId} has been approved by HO and pushed to ERP.");
            });
        }
        return Json(new { success, message = success ? "Request approved and pushed to ERP." : "Failed to approve." });
    }

    [HttpPost]
    public async Task<IActionResult> Reject(decimal reqId, string remarks)
    {
        var user = GetUser();
        var success = await _dbService.HORejectAsync(reqId, user.EmpCode!, remarks ?? "", user.ComCode!);
        if (success)
        {
            _ = Task.Run(async () =>
            {
                var branch = await _dbService.GetRequestBranchAsync(reqId);
                var remarksText = !string.IsNullOrEmpty(remarks) ? $" Remarks: {remarks}" : "";
                if (!string.IsNullOrEmpty(branch))
                    await _notifyService.NotifyZHByBranchAsync(branch, "HO Rejected", $"Request #{reqId} has been rejected by HO ({user.EmpName}).{remarksText}");
                await _notifyService.NotifyRequestCreatorAsync(reqId, "Request Rejected", $"Your request #{reqId} has been rejected by HO.{remarksText}");
            });
        }
        return Json(new { success, message = success ? "Request rejected." : "Failed to reject." });
    }

    [HttpGet]
    public async Task<IActionResult> RejectedByMe()
    {
        var user = GetUser();
        var branchCodes = await GetHOBranchCodesAsync();
        var allRequests = await _dbService.GetHOHistoryAsync(user.ComCode!, branchCodes);
        var rejected = allRequests.Where(r => r.Status == "HO_REJECTED" && r.ActionBy == user.EmpCode).ToList();
        return Json(rejected);
    }

    [HttpGet]
    public async Task<IActionResult> AwaitingHO()
    {
        var user = GetUser();
        var branchCodes = await GetHOBranchCodesAsync();
        var pending = await _dbService.GetHOPendingAsync(user.ComCode!, branchCodes);
        return Json(pending);
    }

    [HttpGet]
    public async Task<IActionResult> ApprovedByMe()
    {
        var user = GetUser();
        var branchCodes = await GetHOBranchCodesAsync();
        var allRequests = await _dbService.GetHOHistoryApproveByMeAsync(user.ComCode!, branchCodes);
        var approved = allRequests.Where(r => r.Status == "HO_APPROVED" && r.ActionBy == user.EmpCode).ToList();
        return Json(approved);
    }

    [HttpGet]
    public async Task<IActionResult> IncreasedRequests()
    {
        var user = GetUser();
        var branchCodes = await GetHOBranchCodesAsync();
        var allRequests = await _dbService.GetHOHistoryIncreaseAndDecrease(user.ComCode!, branchCodes);
        var increased = allRequests.Where(r => r.IncDec == "I" && r.Status == "HO_APPROVED").ToList();
        return Json(increased);
    }

    [HttpGet]
    public async Task<IActionResult> DecreasedRequests()
    {
        var user = GetUser();
        var branchCodes = await GetHOBranchCodesAsync();
        var allRequests = await _dbService.GetHOHistoryIncreaseAndDecrease(user.ComCode!, branchCodes);
        var decreased = allRequests.Where(r => r.IncDec == "D" && r.Status == "HO_APPROVED").ToList();
        return Json(decreased);
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var user = GetUser();
        var branchCodes = await GetHOBranchCodesAsync();
        var list = await _dbService.GetHOHistoryAsync(user.ComCode!, branchCodes);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Reports(DateTime? selectedDate)
    {
        var user = GetUser();
        var date = selectedDate ?? DateTime.Today;
        var previousDate = date.AddDays(-1);
        var branchCodes = await GetHOBranchCodesAsync();

        var todaySummary = await _dbService.GetBranchSummaryByBranchesAsync(user.ComCode!, date, branchCodes);
        var yesterdaySummary = await _dbService.GetBranchSummaryByBranchesAsync(user.ComCode!, previousDate, branchCodes);

        var model = new ReportPageViewModel
        {
            BranchSummary = todaySummary,
            YesterdaySummary = yesterdaySummary,
            SelectedDate = date
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AuditTrail(decimal reqId)
    {
        var trail = await _dbService.GetAuditTrailAsync(reqId);
        return Json(trail);
    }

    [HttpGet]
    public async Task<IActionResult> ApprovalBar(decimal reqId)
    {
        var data = await _dbService.GetRequestApprovalBarAsync(reqId);
        if (data == null) return Json(new { });
        return Json(data);
    }
}
