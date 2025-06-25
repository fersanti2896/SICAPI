
namespace SICAPI.Models.Request.User;

public class CreateUserRequest
{
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MLastName { get; set; }
    public string? Email { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
    public int RoleId { get; set; }
}
