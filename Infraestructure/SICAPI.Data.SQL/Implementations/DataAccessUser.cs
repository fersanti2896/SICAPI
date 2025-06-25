using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.Helpers;
using SICAPI.Models.Request.User;
using SICAPI.Models.Response;
using SICAPI.Models.Response.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SICAPI.Data.SQL.Implementations;

public class DataAccessUser : IDataAccessUser
{
    private IDataAccessLogs IDataAccessLogs;
    private readonly IConfiguration _configuration;
    public AppDbContext Context { get; set; }

    public DataAccessUser(AppDbContext appDbContext, IDataAccessLogs iDataAccessLogs, IConfiguration configurations)
    {
        Context = appDbContext;
        IDataAccessLogs = iDataAccessLogs;
        _configuration = configurations;
    }

    public async Task<ReplyResponse> CreateUser(CreateUserRequest request)
    {
        ReplyResponse response = new();

        try
        {
            var passwordNormalized = request.PasswordHash.ToUpper();
            var encryptedPassword = EncryptDecrypt.EncryptString(passwordNormalized);

            bool userExists = await Context.TUsers.AnyAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (userExists)
            {
                response.Error = new ErrorDTO
                {
                    Code = 400,
                    Message = "Ya existe un usuario con el mismo nombre de usuario o correo."
                };

                return response;
            }

            // Crear nuevo usuario
            var user = new TUsers
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                MLastName = request.MLastName ?? string.Empty,
                Email = request.Email,
                Username = request.Username,
                PasswordHash = encryptedPassword,
                CreditLimit = request.CreditLimit,
                AvailableCredit = request.AvailableCredit,
                RoleId = request.RoleId,
                Status = 1,
                CreateDate = DateTime.Now,
                CreateUser = 1
            };

            await Context.TUsers.AddAsync(user);
            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO()
            {
                Msg = "Usuario creado",
                Status = true
            };

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                Module = "SICAPI-DataAccessUser",
                Action = "User",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };

            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = "Ocurrió un error al procesar la solicitud."
            };
        }

        return response;
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        LoginResponse response = new();

        try
        {
            string passwordNormalized = request.Password.ToUpper();
            string encryptedPassword = EncryptDecrypt.EncryptString(passwordNormalized);

            var user = await Context.TUsers
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == encryptedPassword && u.Status == 1);

            if (user == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Usuario o contraseña inválidos."
                };
                return response;
            }

            var accessToken = GenerateJwtToken(user);
            var existingToken = await Context.TRefreshTokens.FirstOrDefaultAsync(t => t.UserId == user.UserId && !t.IsRevoked && t.ExpirationDate > DateTime.Now);

            string refreshToken;

            if (existingToken != null)
                refreshToken = existingToken.Token;
            else
            {
                refreshToken = GenerateRefreshToken();
                var expiration = DateTime.Now.AddDays(1);

                var newToken = new TRefreshTokens
                {
                    UserId = user.UserId,
                    Token = refreshToken,
                    ExpirationDate = expiration
                };

                await Context.TRefreshTokens.AddAsync(newToken);
                await Context.SaveChangesAsync();
            }

            response.Result = new LoginDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                Token = accessToken,
                RefreshToken = refreshToken,
                FullName = $"{user.FirstName} {user.LastName} {user.MLastName ?? string.Empty}"
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessUser",
                Action = "Login",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = "Error en el proceso de autenticación."
            };
        }

        return response;
    }

    public async Task<LoginResponse> RefreshToken(RefreshTokenRequest request)
    {
        LoginResponse response = new();

        try
        {
            // Buscar refresh token válido en la BD
            var storedToken = await Context.TRefreshTokens.FirstOrDefaultAsync(t => t.Token == request.RefreshToken && !t.IsRevoked && t.ExpirationDate > DateTime.UtcNow);

            if (storedToken is null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 401,
                    Message = "Refresh token inválido o expirado."
                };

                return response;
            }

            // Obtener usuario asociado
            var user = await Context.TUsers.FirstOrDefaultAsync(u => u.UserId == storedToken.UserId && u.Status == 1);

            if (user == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Usuario no encontrado o inactivo."
                };

                return response;
            }

            // Revocar el refresh token anterior
            storedToken.IsRevoked = true;

            // Generar nuevos tokens
            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var newExpiration = DateTime.Now.AddDays(2);

            await Context.TRefreshTokens.AddAsync(new TRefreshTokens
            {
                UserId = user.UserId,
                Token = newRefreshToken,
                ExpirationDate = newExpiration
            });

            await Context.SaveChangesAsync();

            // Devolver nuevos tokens
            response.Result = new LoginDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                FullName = $"{user.FirstName} {user.LastName} {user.MLastName ?? string.Empty}"
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessUser",
                Action = "RefreshToken",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = "Error al procesar el refresh token."
            };
        }

        return response;
    }

    private string GenerateJwtToken(TUsers user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim("UserId", user.UserId.ToString()),
            new Claim("Role", user.RoleId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtSettings["ExpiryMinutes"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
}
