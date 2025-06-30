namespace SICAPI.Models.DTOs;

public class SupplierDTO
{
    public int SupplierId { get; set; }
    public string BusinessName { get; set; } 
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public int Status { get; set; }
    public string DescriptionStatus { get; set; }
}
