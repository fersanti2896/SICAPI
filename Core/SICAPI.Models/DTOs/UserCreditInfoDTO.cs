namespace SICAPI.Models.DTOsc;

public class UserCreditInfoDTO
{
    public int UserId { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
}
