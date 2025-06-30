namespace SICAPI.Models.DTOs;

public class UserDTO
{
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MLastName { get; set; }
    public string Username { get; set; }
    public int Status { get; set; }
    public string DescriptionStatus { get; set; }
    public string Role { get; set; }
}
