namespace DCRSupplyApp.Models;

public class UserSessionModel
{
    public string? UserId { get; set; }
    public string? HrCode { get; set; }
    public string? ComCode { get; set; }
    public string? EmpCode { get; set; }
    public string? EmpName { get; set; }
    public string? Designation { get; set; }
    public string? BranchCode { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Zone { get; set; }
    public string? ReportTo { get; set; }
    public string? RoleName { get; set; }
    public string? HierarchyCode { get; set; }
    public string? SelectedRole { get; set; }
    public string? UnitCode { get; set; }
    public List<BranchDetail> BranchDetails { get; set; }
    public List<RoleDetails> RoleDetails { get; set; }
}

public class BranchDetail
{
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; } 
}

public class RoleDetails
{
    public string? RoleId { get; set; }
    public string? RoleName { get; set; }
}