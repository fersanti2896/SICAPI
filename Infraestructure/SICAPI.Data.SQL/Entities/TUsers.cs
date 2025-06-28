using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SICAPI.Data.SQL.Entities;

[Table("TUsers")]
public class TUsers : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MLastName { get; set; }
    public string? Email { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public int RoleId { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }

    public virtual TRol? Role { get; set; }
}
