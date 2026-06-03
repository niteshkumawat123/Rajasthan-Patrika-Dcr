namespace DCRSupplyApp.Models;

public class ZHDashboardViewModel
{
    public int AwaitingMe { get; set; }
    public int AtHo { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public List<SupplyRequestViewModel> PendingRequests { get; set; } = new();
    public UserSessionModel? User { get; set; }
    public bool CanAddRequest { get; set; }
    public bool CanApprove { get; set; }
}
