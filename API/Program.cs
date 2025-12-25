using Application.DTOs.Auth;
using Application.DTOs.User;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ReclamosContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBankAccountValidator, BankAccountValidator>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowReactApp");

app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.AuthenticateAsync(request);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
})
.AllowAnonymous();

app.MapPost("/api/users/create", [Authorize] async (CreateUserRequest request, IUserService userService, HttpContext httpContext) =>
{
    try
    {
        var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userRole))
            return Results.Unauthorized();

        var response = await userService.CreateUserAsync(request, userRole);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Forbid();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error interno del servidor: {ex.Message}");
    }
});

app.MapGet("/api/users/allowed-roles", [Authorize] (IUserService userService, HttpContext httpContext) =>
{
    var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

    if (string.IsNullOrEmpty(userRole))
        return Results.Unauthorized();

    var allowedRoles = new List<string>();

    switch (userRole)
    {
        case "Revisor":
            allowedRoles = new List<string> { "Cliente", "Revisor" };
            break;
        case "Tecnico":
            allowedRoles = new List<string> { "Tecnico" };
            break;
        case "Personal de Entrega":
            allowedRoles = new List<string> { "Personal de Entrega" };
            break;
        default:
            allowedRoles = new List<string>();
            break;
    }

    return Results.Ok(allowedRoles);
});

app.Run();