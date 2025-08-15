using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SICAPI.Data.SQL.Entities;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Models.DTOs;
using SICAPI.Models.DTOsc;
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
    private static readonly TimeZoneInfo _cdmxZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
    private static DateTime NowCDMX => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _cdmxZone);

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

            bool userExists = await Context.TUsers.AnyAsync(u => u.Username == request.Username);

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
                CreateDate = NowCDMX,
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

            var user = await Context.TUsers.Include(u => u.Role)
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
            var refreshToken = GenerateRefreshToken();
            var expiration = NowCDMX.AddDays(1);

            var newToken = new TRefreshTokens
            {
                UserId = user.UserId,
                Token = refreshToken,
                ExpirationDate = expiration,
                IsRevoked = false
            };

            await Context.TRefreshTokens.AddAsync(newToken);
            await Context.SaveChangesAsync();

            //var existingToken = await Context.TRefreshTokens.FirstOrDefaultAsync(t => t.UserId == user.UserId && !t.IsRevoked && t.ExpirationDate > DateTime.Now);


            //if (existingToken != null)
            //    refreshToken = existingToken.Token;
            //else
            //{
            //    refreshToken = GenerateRefreshToken();
            //    var expiration = DateTime.Now.AddDays(1);

            //    var newToken = new TRefreshTokens
            //    {
            //        UserId = user.UserId,
            //        Token = refreshToken,
            //        ExpirationDate = expiration
            //    };

            //    await Context.TRefreshTokens.AddAsync(newToken);
            //    await Context.SaveChangesAsync();
            //}

            response.Result = new LoginDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                Token = accessToken,
                RefreshToken = refreshToken,
                FullName = $"{user.FirstName} {user.LastName} {user.MLastName ?? string.Empty}",
                RoleId = user.RoleId,
                RoleDescription = user.Role?.Name
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
            var storedToken = await Context.TRefreshTokens.FirstOrDefaultAsync(t => t.Token == request.RefreshToken && !t.IsRevoked && t.ExpirationDate > NowCDMX);

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
            var user = await Context.TUsers.Include(u => u.Role)
                                           .FirstOrDefaultAsync(u => u.UserId == storedToken.UserId && u.Status == 1);

            if (user == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Usuario no encontrado o inactivo."
                };

                return response;
            }

            // se comenta el revocar el token para que se pueda iniciar en multiples dispositivos
            //storedToken.IsRevoked = true;

            // Generar nuevos tokens
            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var newExpiration = NowCDMX.AddDays(2);

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
                FullName = $"{user.FirstName} {user.LastName} {user.MLastName ?? string.Empty}",
                RoleId = user.RoleId,
                RoleDescription = user.Role?.Name
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
            expires: NowCDMX.AddMinutes(Convert.ToInt32(jwtSettings["ExpiryMinutes"])),
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

    public async Task<UsersReponse> GetAllUsers(int userId)
    {
        UsersReponse response = new();
        try
        {
            var users = await Context.TUsers
                                     .Include(u => u.Role)
                                     .Select(u => new UserDTO
                                     {
                                        UserId = u.UserId,
                                        FirstName = u.FirstName,
                                        LastName = u.LastName,
                                        MLastName = u.MLastName,
                                        Username = u.Username,
                                        Status = u.Status,
                                        DescriptionStatus = u.Status == 1 ? "Activo" : "Inactivo",
                                        Role = u.Role.Name,
                                        Email = u.Email,
                                        CreditLimit = u.CreditLimit,
                                        RoleId = u.RoleId
                                     })
                                     .ToListAsync();

            response.Result = users;

            return response;
        }
        catch (Exception ex)
        {
            return new UsersReponse
            {
                Result = null,
                Error = new ErrorDTO
                {
                    Code = 500,
                    Message = $"Error Exception: {ex.InnerException}"
                }
            };
        }
    }


    public async Task<ReplyResponse> DeactivateUser(ActivateUserRequest request, int userId)
    {
        var response = new ReplyResponse();

        try
        {
            var user = await Context.TUsers.FirstOrDefaultAsync(u => u.Username == request.UserName);

            if (user == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Usuario no encontrado"
                };
                return response;
            }

            user.Status = request.Status;
            user.UpdateDate = NowCDMX;
            user.UpdateUser = userId;

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Msg = request.Status == 1 ? "Usuario activado correctamente" : "Usuario desactivado correctamente",
                Status = true
            };
        }
        catch (Exception ex)
        {
            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error al desactivar usuario: {ex.Message}"
            };
        }

        return response;
    }

    public async Task<UserInfoResponse> CreditInfo(int UserId)
    {
        UserInfoResponse response = new();

        try
        {
            var user = await Context.TUsers.Where(u => u.UserId == UserId)
                                            .Select(u => new UserCreditInfoDTO
                                            {
                                                UserId = u.UserId,
                                                CreditLimit = u.CreditLimit,
                                                AvailableCredit = u.AvailableCredit
                                            }).FirstOrDefaultAsync();

            response.Result = user;

            return response;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessUser",
                Action = "CreditInfo",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            return new UserInfoResponse
            {
                Result = null,
                Error = new ErrorDTO
                {
                    Code = 500,
                    Message = $"Error Exception: {ex.InnerException}"
                }
            };
        }
    }

    public async Task<ReplyResponse> UpdateUser(UpdateUserRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            var user = await Context.TUsers.FirstOrDefaultAsync(u => u.UserId == request.UserId);

            if (user == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Usuario no encontrado."
                };
                return response;
            }

            // Validar si otro usuario ya tiene el mismo username
            var usernameExists = await Context.TUsers
                .AnyAsync(u => u.UserId != request.UserId && u.Username == request.Username);

            if (usernameExists)
            {
                response.Error = new ErrorDTO
                {
                    Code = 400,
                    Message = "Ya existe otro usuario con el mismo nombre de usuario."
                };
                return response;
            }

            // Actualizar campos
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.MLastName = request.MLastName ?? string.Empty;
            user.Email = request.Email;
            user.Username = request.Username;

            if (request.CreditLimit != user.CreditLimit)
            {
                var usedCredit = user.CreditLimit - user.AvailableCredit;
                user.CreditLimit = request.CreditLimit;
                user.AvailableCredit = user.CreditLimit - usedCredit;

                if (user.AvailableCredit < 0)
                    user.AvailableCredit = 0; 
            }
            else
            {
                user.CreditLimit = request.CreditLimit;
                user.AvailableCredit = request.AvailableCredit;
            }

            user.RoleId = request.RoleId;
            user.UpdateDate = NowCDMX;
            user.UpdateUser = userId;

            if (!string.IsNullOrWhiteSpace(request.PasswordHash))
            {
                var normalizedPassword = request.PasswordHash.ToUpper();
                user.PasswordHash = EncryptDecrypt.EncryptString(normalizedPassword);
            }

            await Context.SaveChangesAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Usuario actualizado correctamente."
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "SICAPI-DataAccessUser",
                Action = "UpdateUserAsync",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = "Error al actualizar el usuario."
            };
        }

        return response;
    }
}
