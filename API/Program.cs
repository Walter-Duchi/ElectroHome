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

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

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
builder.Services.AddScoped<IEmailService, EmailService>();
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

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.AuthenticateAsync(request);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error interno: {ex.Message}");
    }
})
.AllowAnonymous();

app.MapPost("/api/auth/forgot-password", async (ForgotPasswordRequest request, IAuthService authService) =>
{
    try
    {
        app.Logger.LogInformation($"Solicitud de restablecimiento para: {request.Correo}");

        var result = await authService.RequestPasswordResetAsync(request.Correo);

        return Results.Ok(new
        {
            message = "Si el correo existe en nuestro sistema, recibirás instrucciones para restablecer tu contraseña en unos minutos.",
            success = true
        });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, $"Error en forgot-password: {ex.Message}");
        return Results.Problem($"Error interno del servidor. Por favor, intenta nuevamente más tarde.");
    }
})
.AllowAnonymous();

app.MapGet("/api/auth/validate-reset-token", async (string token, IAuthService authService) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Results.BadRequest(new { valid = false, message = "Token no proporcionado." });
        }

        var isValid = await authService.ValidateResetTokenAsync(token);

        return Results.Ok(new
        {
            valid = isValid,
            message = isValid ? "Token válido." : "Token inválido o expirado."
        });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, $"Error en validate-reset-token: {ex.Message}");
        return Results.Problem($"Error interno del servidor.");
    }
})
.AllowAnonymous();

app.MapPost("/api/auth/reset-password", async (ResetPasswordRequest request, IAuthService authService) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Results.BadRequest(new { message = "Token requerido." });
        }

        if (request.NuevaContrasena != request.ConfirmarContrasena)
        {
            return Results.BadRequest(new { message = "Las contraseñas no coinciden." });
        }

        var result = await authService.ResetPasswordAsync(request.Token, request.NuevaContrasena);

        if (result)
        {
            return Results.Ok(new
            {
                message = "¡Contraseña restablecida exitosamente! Ya puedes iniciar sesión con tu nueva contraseña.",
                success = true
            });
        }

        return Results.BadRequest(new
        {
            message = "Token inválido, expirado o ya utilizado. Por favor, solicita un nuevo enlace de restablecimiento.",
            success = false
        });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, $"Error en reset-password: {ex.Message}");
        return Results.Problem($"Error interno del servidor. Por favor, intenta nuevamente.");
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