namespace DCRSupplyApp.Models;

public class DashboardViewModel
{
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Today { get; set; }
    public List<SupplyRequestViewModel> RecentRequests { get; set; } = new();
    public UserSessionModel? User { get; set; }
}
