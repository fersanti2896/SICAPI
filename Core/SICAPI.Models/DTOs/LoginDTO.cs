namespace SICAPI.Models.DTOs;

public class LoginDTO
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public string FullName { get; set; }
    public int RoleId { get; set; }
    public string? RoleDescription { get; set; }
}
