using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SICAPI.Data.SQL;
using SICAPI.Data.SQL.Implementations;
using SICAPI.Data.SQL.Interfaces;
using SICAPI.Infrastructure.Implementations;
using SICAPI.Infrastructure.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionLocal"),
       o => o.UseCompatibilityLevel(120).CommandTimeout(Convert.ToInt32(TimeSpan.FromMinutes(10).TotalSeconds))));

// Core
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ISupplierRepository, SupplierRepository>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<IClientRepository, ClientRepository>();
builder.Services.AddTransient<ICatalogsRepository, CatalogsRepository>();
builder.Services.AddTransient<ISalesRepository, SalesRepository>();

// Infraestructure
builder.Services.AddTransient<IDataAccessUser, DataAccessUser>();
builder.Services.AddTransient<IDataAccessLogs, DataAccessLogs>();
builder.Services.AddTransient<IDataAccessSupplier, DataAccessSupplier>();
builder.Services.AddTransient<IDataAccessProduct, DataAccessProduct>();
builder.Services.AddTransient<IDataAccessClient, DataAccessClient>();
builder.Services.AddTransient<IDataAccessCatalogs, DataAccessCatalogs>();
builder.Services.AddTransient<IDataAccessSales, DataAccessSales>();

// Add services to the container.
builder.Services.AddControllers();

var corsPolicy = "AllowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy,
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SICAPI ", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Encabezado de autorizacion JSON Web Token utilizando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(corsPolicy);
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
