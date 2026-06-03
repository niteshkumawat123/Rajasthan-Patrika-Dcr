using Org.BouncyCastle.Asn1;

namespace DCRSupplyApp.Models;

public class SupplyRequestViewModel
{
    public decimal? ReqId { get; set; }
    public string? CompCode { get; set; }
    public string? UnitCode { get; set; }
    public string? Agcd { get; set; }
    public string? Dpcd { get; set; }
    public string? Publ { get; set; }
    public string? Edtn { get; set; }
    public string? SupplyTypeCode { get; set; }
    public decimal? BaseSupply { get; set; }
    public string? IncDec { get; set; }
    public decimal? ChangedSupply { get; set; }
    public string? ReasonCode { get; set; }
    public string? ZoneCode { get; set; }
    public string? UserId { get; set; }
    public DateTime? CreationDate { get; set; }
    public DateTime? ChangedSupplyDate { get; set; }
    public string? Remarks { get; set; }
    public string? Status { get; set; }
    public string? ErpPushFlag { get; set; }
    public DateTime? ErpPushDate { get; set; }
    public string? AgName { get; set; }
    public string? ApprAction { get; set; }
    public string? ActionBy { get; set; }
    public DateTime? ActionDate { get; set; }
    public string? ApproverRemarks { get; set; }
    public string? BranchCode { get; set; }
    public string? ZhApprovedBy { get; set; }
    public string? ZhRemarks { get; set; }
    public DateTime? ZhActionDate { get; set; }
    public string? CreationBy { get; set; }
    public string? CreationByCode { get; set; }
    public string? EmployeeCode { get; set; }
    public string? DropPointName { get; set; }

    // Day-wise supply fields
    public int? SupplyMon { get; set; }
    public int? SupplyTue { get; set; }
    public int? SupplyWed { get; set; }
    public int? SupplyThu { get; set; }
    public int? SupplyFri { get; set; }
    public int? SupplySat { get; set; }
    public int? SupplySun { get; set; }
    public bool DayWiseEnabled { get; set; }
}
