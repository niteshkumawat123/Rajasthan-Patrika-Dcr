namespace DCRSupplyApp.Models;

public class SupplyViewModel
{
    public int? BaseSupply { get; set; }
    public int? SupplyMon { get; set; }
    public int? SupplyTue { get; set; }
    public int? SupplyWed { get; set; }
    public int? SupplyThu { get; set; }
    public int? SupplyFri { get; set; }
    public int? SupplySat { get; set; }
    public int? SupplySun { get; set; }
    public DateTime? SupplyEffectiveDate { get; set; }
    public string? SupplyFlag { get; set; }
    public string? Publ { get; set; }
    public string? Edtn { get; set; }
    public string? SupplyTypeCode { get; set; }
    public string? droppointname { get; set; }
}
