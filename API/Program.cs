using Application.DTOs.Auth;
using Application.DTOs.Reclamo;
using Application.DTOs.Tecnico;
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
builder.Services.AddScoped<IReclamoService, ReclamoService>();
builder.Services.AddScoped<ITecnicoService, TecnicoService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:5298", "http://localhost:3000")
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

// Endpoints para reclamos
app.MapPost("/api/reclamos/validar-cliente", [Authorize(Roles = "Revisor")] async (ValidarClienteRequest request, IReclamoService reclamoService) =>
{
    var response = await reclamoService.ValidarClienteAsync(request.Ruc);
    return response.EsValido ? Results.Ok(response) : Results.BadRequest(response);
});

app.MapPost("/api/reclamos/validar-producto", [Authorize(Roles = "Revisor")] async (ValidarProductoRequest request, IReclamoService reclamoService) =>
{
    var response = await reclamoService.ValidarProductoAsync(request.NumeroSerie);
    return response.EsValido ? Results.Ok(response) : Results.BadRequest(response);
});

app.MapPost("/api/reclamos/crear", [Authorize(Roles = "Revisor")] async (CrearReclamoRequest request, IReclamoService reclamoService, HttpContext httpContext) =>
{
    var revisorId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    var response = await reclamoService.CrearReclamoAsync(request, revisorId);
    return response.Exito ? Results.Ok(response) : Results.BadRequest(response);
});

// ============================================
// ENDPOINTS PARA TÉCNICO
// ============================================

// Obtener productos asignados al técnico
app.MapGet("/api/tecnico/productos", [Authorize(Roles = "Tecnico")] async (HttpContext httpContext, ITecnicoService tecnicoService) =>
{
    try
    {
        var tecnicoId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var productos = await tecnicoService.ObtenerProductosAsignadosAsync(tecnicoId);
        return Results.Ok(productos);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al obtener productos del técnico");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Obtener próximo producto a revisar
app.MapGet("/api/tecnico/proximo-producto", [Authorize(Roles = "Tecnico")] async (HttpContext httpContext, ITecnicoService tecnicoService) =>
{
    try
    {
        var tecnicoId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var producto = await tecnicoService.ObtenerProximoProductoAsync(tecnicoId);

        if (producto == null)
        {
            return Results.NotFound(new { message = "No hay productos pendientes para revisar" });
        }

        return Results.Ok(producto);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al obtener próximo producto");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Validar si un producto está en el orden correcto para revisar
app.MapGet("/api/tecnico/validar-orden/{id}", [Authorize(Roles = "Tecnico")] async (int id, HttpContext httpContext, ITecnicoService tecnicoService) =>
{
    try
    {
        var tecnicoId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var valido = await tecnicoService.ValidarOrdenRevisacionAsync(id, tecnicoId);

        return Results.Ok(new
        {
            valido = valido,
            message = valido ? "Producto en orden correcto para revisión" : "No está en el orden correcto para revisión"
        });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al validar orden de revisión");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Iniciar revisión de un producto
app.MapPost("/api/tecnico/iniciar-revision", [Authorize(Roles = "Tecnico")] async (IniciarRevisionRequest request, HttpContext httpContext, ITecnicoService tecnicoService) =>
{
    try
    {
        var tecnicoId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        request.TecnicoId = tecnicoId;

        var resultado = await tecnicoService.IniciarRevisionAsync(request);

        if (!resultado)
        {
            return Results.BadRequest(new { message = "No se pudo iniciar la revisión. Verifique que sea el producto más antiguo y que no tenga otra revisión activa." });
        }

        return Results.Ok(new { message = "Revisión iniciada exitosamente" });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al iniciar revisión");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Finalizar revisión de un producto
app.MapPost("/api/tecnico/finalizar-revision", [Authorize(Roles = "Tecnico")] async (FinalizarRevisionRequest request, HttpContext httpContext, ITecnicoService tecnicoService) =>
{
    try
    {
        var tecnicoId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        request.TecnicoId = tecnicoId;

        // Validaciones adicionales
        if (request.Estado != "Aprobado" && request.Estado != "Rechazado")
        {
            return Results.BadRequest(new { message = "Estado inválido. Debe ser 'Aprobado' o 'Rechazado'" });
        }

        if (string.IsNullOrWhiteSpace(request.Explicacion))
        {
            return Results.BadRequest(new { message = "La explicación es requerida" });
        }

        var resultado = await tecnicoService.FinalizarRevisionAsync(request);

        if (!resultado)
        {
            return Results.BadRequest(new { message = "No se pudo finalizar la revisión" });
        }

        return Results.Ok(new { message = "Revisión finalizada exitosamente" });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al finalizar revisión");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

app.Run();