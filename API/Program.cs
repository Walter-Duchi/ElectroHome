using Application.DTOs.Auth;
using Application.DTOs.Entrega;
using Application.DTOs.Reclamo;
using Application.DTOs.Tecnico;
using Application.DTOs.User;
using Application.DTOs.Entrega;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuración de logging EXTENDIDO
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Aumentar verbosidad para todo
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Trace);
});

// Habilitar logs detallados de Entity Framework
builder.Services.AddDbContext<ReclamosContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging(true);
    options.EnableDetailedErrors(true);
    options.LogTo(Console.WriteLine, LogLevel.Information);
});

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
    
    // Agregar logging para JWT
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"JWT Token validated for user: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Servicios
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBankAccountValidator, BankAccountValidator>();
builder.Services.AddScoped<IReclamoService, ReclamoService>();
builder.Services.AddScoped<ITecnicoService, TecnicoService>();
builder.Services.AddScoped<IEntregaService, EntregaService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:5298", "http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddControllers();

var app = builder.Build();

// Middleware para logging de todas las peticiones
app.Use(async (context, next) =>
{
    var logger = app.Logger;
    var request = context.Request;
    
    logger.LogInformation("=========================================");
    logger.LogInformation("NUEVA PETICIÓN HTTP");
    logger.LogInformation("Método: {Method}", request.Method);
    logger.LogInformation("Path: {Path}", request.Path);
    logger.LogInformation("QueryString: {QueryString}", request.QueryString);
    logger.LogInformation("Content-Type: {ContentType}", request.ContentType);
    logger.LogInformation("Headers:");
    foreach (var header in request.Headers)
    {
        logger.LogInformation("  {Header}: {Value}", header.Key, header.Value);
    }
    logger.LogInformation("=========================================");
    
    await next();
    
    logger.LogInformation("RESPUESTA: Status {StatusCode}", context.Response.StatusCode);
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "EXCEPCIÓN NO MANEJADA: {Message}", ex.Message);
            
            // Log detallado
            logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
            logger.LogError("Inner Exception: {InnerException}", ex.InnerException?.Message);
            
            throw;
        }
    });
}

app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

// Middleware para logging de rutas disponibles (solo desarrollo)
if (app.Environment.IsDevelopment())
{
    app.Map("/debug/routes", endpoints =>
    {
        endpoints.Run(async context =>
        {
            var endpointDataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();
            
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Rutas disponibles en la API",
                routes = endpointDataSource.Endpoints
                    .OfType<RouteEndpoint>()
                    .Select(e => new
                    {
                        method = e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods?[0] ?? "ANY",
                        pattern = e.RoutePattern.RawText,
                        displayName = e.DisplayName
                    })
                    .ToList()
            });
        });
    });
}

// ============================================
// ENDPOINTS DE AUTENTICACIÓN
// ============================================
app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService, ILogger<Program> logger) =>
{
    logger.LogInformation("Intento de login para: {Correo}", request.Correo);
    try
    {
        var response = await authService.AuthenticateAsync(request);
        logger.LogInformation("Login exitoso para: {Correo}", request.Correo);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
        logger.LogWarning("Login fallido para: {Correo} - {Mensaje}", request.Correo, ex.Message);
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error interno en login para: {Correo}", request.Correo);
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
// ENDPOINTS PARA TÉCNICO - CON LOGGING DETALLADO
// ============================================

// Obtener productos asignados al técnico
app.MapGet("/api/tecnico/productos", [Authorize(Roles = "Tecnico")] async (HttpContext httpContext, ITecnicoService tecnicoService) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("=== INICIO ENDPOINT /api/tecnico/productos ===");
        
        // Obtener ID del técnico del token
        var user = httpContext.User;
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        
        logger.LogInformation("Usuario autenticado - ID: {UserId}, Rol: {UserRole}", userIdClaim, userRole);
        
        if (string.IsNullOrEmpty(userIdClaim))
        {
            logger.LogWarning("No se encontró ID en el token JWT");
            return Results.Unauthorized();
        }
        
        var tecnicoId = int.Parse(userIdClaim);
        logger.LogInformation("Obteniendo productos para técnico ID: {TecnicoId}", tecnicoId);
        
        var productos = await tecnicoService.ObtenerProductosAsignadosAsync(tecnicoId);
        
        logger.LogInformation("Productos obtenidos: {Count}", productos.Count);
        if (productos.Count > 0)
        {
            foreach (var producto in productos)
            {
                logger.LogInformation("Producto: {Id} - {NumeroSerie} - {Estado}", 
                    producto.Id, producto.NumeroSerie, producto.Estado);
            }
        }
        
        logger.LogInformation("=== FIN ENDPOINT /api/tecnico/productos ===");
        return Results.Ok(productos);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "ERROR en /api/tecnico/productos: {Message}", ex.Message);
        logger.LogError("Stack Trace: {StackTrace}", ex.StackTrace);
        
        return Results.Problem(
            detail: $"Error interno: {ex.Message}",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Error del servidor");
    }
}).WithName("GetTecnicoProductos");

// Obtener próximo producto a revisar
app.MapGet("/api/tecnico/proximo-producto", [Authorize(Roles = "Tecnico")] async (HttpContext httpContext, ITecnicoService tecnicoService) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("=== INICIO ENDPOINT /api/tecnico/proximo-producto ===");
        
        var tecnicoId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        logger.LogInformation("Buscando próximo producto para técnico ID: {TecnicoId}", tecnicoId);
        
        var producto = await tecnicoService.ObtenerProximoProductoAsync(tecnicoId);

        if (producto == null)
        {
            logger.LogInformation("No hay productos pendientes para técnico ID: {TecnicoId}", tecnicoId);
            return Results.NotFound(new { message = "No hay productos pendientes para revisar" });
        }

        logger.LogInformation("Próximo producto encontrado: ID={Id}, Serie={NumeroSerie}", 
            producto.Id, producto.NumeroSerie);
        
        logger.LogInformation("=== FIN ENDPOINT /api/tecnico/proximo-producto ===");
        return Results.Ok(producto);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error en /api/tecnico/proximo-producto");
        return Results.Problem($"Error interno: {ex.Message}");
    }
}).WithName("GetProximoProducto");

// Validar si un producto está en el orden correcto para revisar
app.MapGet("/api/tecnico/validar-orden/{id}", [Authorize(Roles = "Tecnico")] async (int id, HttpContext httpContext, ITecnicoService tecnicoService) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Validando orden para producto ID: {ProductoId}", id);
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
        logger.LogError(ex, "Error al validar orden de revisión para producto ID: {ProductoId}", id);
        return Results.Problem($"Error interno: {ex.Message}");
    }
}).WithName("ValidarOrdenRevision");

// Iniciar revisión de un producto
app.MapPost("/api/tecnico/iniciar-revision", [Authorize(Roles = "Tecnico")] async (IniciarRevisionRequest request, HttpContext httpContext, ITecnicoService tecnicoService) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("=== INICIAR REVISIÓN ===");
        logger.LogInformation("Request: {@Request}", request);
        
        var tecnicoId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        request.TecnicoId = tecnicoId;

        logger.LogInformation("Técnico ID: {TecnicoId} intentando iniciar revisión de producto ID: {ProductoId}", 
            tecnicoId, request.ReclamoProductoSnId);

        var resultado = await tecnicoService.IniciarRevisionAsync(request);

        if (!resultado)
        {
            logger.LogWarning("No se pudo iniciar la revisión para producto ID: {ProductoId}", request.ReclamoProductoSnId);
            return Results.BadRequest(new { message = "No se pudo iniciar la revisión. Verifique que sea el producto más antiguo y que no tenga otra revisión activa." });
        }

        logger.LogInformation("Revisión iniciada exitosamente para producto ID: {ProductoId}", request.ReclamoProductoSnId);
        return Results.Ok(new { message = "Revisión iniciada exitosamente" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al iniciar revisión para producto ID: {ProductoId}", request?.ReclamoProductoSnId);
        return Results.Problem($"Error interno: {ex.Message}");
    }
}).WithName("IniciarRevision");

// Finalizar revisión de un producto
app.MapPost("/api/tecnico/finalizar-revision", [Authorize(Roles = "Tecnico")] async (FinalizarRevisionRequest request, HttpContext httpContext, ITecnicoService tecnicoService) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("=== FINALIZAR REVISIÓN ===");
        logger.LogInformation("Request: {@Request}", request);
        
        var tecnicoId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        request.TecnicoId = tecnicoId;

        logger.LogInformation("Técnico ID: {TecnicoId} finalizando revisión de producto ID: {ProductoId}", 
            tecnicoId, request.ReclamoProductoSnId);

        // Validaciones adicionales
        if (request.Estado != "Aprobado" && request.Estado != "Rechazado")
        {
            logger.LogWarning("Estado inválido: {Estado}", request.Estado);
            return Results.BadRequest(new { message = "Estado inválido. Debe ser 'Aprobado' o 'Rechazado'" });
        }

        if (string.IsNullOrWhiteSpace(request.Explicacion))
        {
            logger.LogWarning("Explicación vacía para producto ID: {ProductoId}", request.ReclamoProductoSnId);
            return Results.BadRequest(new { message = "La explicación es requerida" });
        }

        var resultado = await tecnicoService.FinalizarRevisionAsync(request);

        if (!resultado)
        {
            logger.LogWarning("No se pudo finalizar la revisión para producto ID: {ProductoId}", request.ReclamoProductoSnId);
            return Results.BadRequest(new { message = "No se pudo finalizar la revisión" });
        }

        logger.LogInformation("Revisión finalizada exitosamente para producto ID: {ProductoId}. Estado: {Estado}", 
            request.ReclamoProductoSnId, request.Estado);
        return Results.Ok(new { message = "Revisión finalizada exitosamente" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al finalizar revisión para producto ID: {ProductoId}", request?.ReclamoProductoSnId);
        return Results.Problem($"Error interno: {ex.Message}");
    }
}).WithName("FinalizarRevision");

// ============================================
// ENDPOINT PARA DEBUG - Ver todas las rutas
// ============================================
app.MapGet("/debug/endpoints", (IEnumerable<EndpointDataSource> endpointSources) =>
{
    var endpoints = endpointSources.SelectMany(es => es.Endpoints);
    var result = new List<object>();
    
    foreach (var endpoint in endpoints)
    {
        if (endpoint is RouteEndpoint routeEndpoint)
        {
            result.Add(new
            {
                Pattern = routeEndpoint.RoutePattern.RawText,
                Methods = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods,
                DisplayName = routeEndpoint.DisplayName
            });
        }
    }
    
    return Results.Ok(result);
}).AllowAnonymous();

// Endpoint diagnóstico DB
app.MapGet("/api/diagnostico", async (ReclamosContext context, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("=== DIAGNÓSTICO DEL SISTEMA ===");
        
        // Verificar conexión a BD
        var canConnect = await context.Database.CanConnectAsync();
        logger.LogInformation("Conexión a BD: {CanConnect}", canConnect);
        
        // Contar registros en tablas clave
        var tecnicosCount = await context.Usuarios.CountAsync(u => u.Rol == "Tecnico");
        var productosCount = await context.ReclamosProductoSns.CountAsync();
        var productosConTecnico = await context.ReclamosProductoSns
            .Where(rps => rps.FkTecnicoAsignado != null)
            .CountAsync();
        
        logger.LogInformation("Técnicos en BD: {TecnicosCount}", tecnicosCount);
        logger.LogInformation("Total productos en reclamos: {ProductosCount}", productosCount);
        logger.LogInformation("Productos con técnico asignado: {ConTecnico}", productosConTecnico);
        
        // Listar técnicos
        var tecnicos = await context.Usuarios
            .Where(u => u.Rol == "Tecnico")
            .Select(u => new { u.Id, u.Nombres, u.Apellidos, u.Correo })
            .ToListAsync();
            
        // Listar productos asignados
        var productosAsignados = await context.ReclamosProductoSns
            .Include(rps => rps.FkTecnicoAsignadoNavigation)
            .Where(rps => rps.FkTecnicoAsignado != null)
            .Select(rps => new 
            { 
                rps.Id,
                NumeroSerie = rps.FkNumeroSerieProductosNavigation.NumeroSerie,
                TécnicoId = rps.FkTecnicoAsignado,
                TécnicoNombre = rps.FkTecnicoAsignadoNavigation.Nombres + " " + rps.FkTecnicoAsignadoNavigation.Apellidos,
                rps.Estado
            })
            .ToListAsync();
        
        return Results.Ok(new
        {
            timestamp = DateTime.Now,
            database = new
            {
                connected = canConnect,
                connectionString = context.Database.GetConnectionString()
            },
            counts = new
            {
                tecnicos = tecnicosCount,
                productos = productosCount,
                productosConTecnico = productosConTecnico
            },
            tecnicos = tecnicos,
            productosAsignados = productosAsignados
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error en diagnóstico");
        return Results.Problem($"Error en diagnóstico: {ex.Message}");
    }
}).AllowAnonymous();


// ============================================
// ENDPOINTS PARA PERSONAL DE ENTREGA
// ============================================

// Buscar reclamo para entrega
app.MapPost("/api/entrega/buscar-reclamo", [Authorize(Roles = "Personal de Entrega")] async (
    BuscarReclamoRequest request,
    IEntregaService entregaService,
    HttpContext httpContext) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation($"Buscando reclamo para entrega: {request.CodigoReclamo}");
        var response = await entregaService.BuscarReclamoAsync(request.CodigoReclamo);
        return response.Exito ? Results.Ok(response) : Results.BadRequest(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error al buscar reclamo: {request.CodigoReclamo}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Validar producto de reemplazo
app.MapPost("/api/entrega/validar-reemplazo", [Authorize(Roles = "Personal de Entrega")] async (
    ValidarReemplazoRequest request,
    IEntregaService entregaService,
    HttpContext httpContext) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation($"Validando producto de reemplazo: {request.NumeroSerieReemplazo}");
        var response = await entregaService.ValidarProductoReemplazoAsync(
            request.ReclamoProductoSnId,
            request.NumeroSerieReemplazo);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error al validar reemplazo: {request.NumeroSerieReemplazo}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Seleccionar producto de reemplazo
app.MapPost("/api/entrega/seleccionar-reemplazo", [Authorize(Roles = "Personal de Entrega")] async (
    SeleccionarReemplazoRequest request,
    IEntregaService entregaService,
    HttpContext httpContext) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        var personalEntregaId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        logger.LogInformation($"Seleccionando reemplazo para producto: {request.ReclamoProductoSnId}");

        var resultado = await entregaService.SeleccionarReemplazoAsync(request, personalEntregaId);
        return resultado ?
            Results.Ok(new { message = "Reemplazo seleccionado exitosamente" }) :
            Results.BadRequest(new { message = "No se pudo seleccionar el reemplazo" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error al seleccionar reemplazo: {request.ReclamoProductoSnId}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Verificar si todos los productos tienen reemplazo
app.MapGet("/api/entrega/verificar-reemplazos/{codigoReclamo}", [Authorize(Roles = "Personal de Entrega")] async (
    string codigoReclamo,
    IEntregaService entregaService,
    HttpContext httpContext) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation($"Verificando reemplazos para: {codigoReclamo}");
        var todosTienenReemplazo = await entregaService.TodosProductosTienenReemplazoAsync(codigoReclamo);
        return Results.Ok(new { todosTienenReemplazo });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error al verificar reemplazos: {codigoReclamo}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Generar datos para comprobante
app.MapPost("/api/entrega/generar-datos-comprobante", [Authorize(Roles = "Personal de Entrega")] async (
    GenerarComprobanteRequest request,
    IEntregaService entregaService,
    HttpContext httpContext) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        var personalEntregaId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        logger.LogInformation($"Generando datos para comprobante: {request.CodigoReclamo}");

        var comprobante = await entregaService.GenerarDatosComprobanteAsync(request.CodigoReclamo, personalEntregaId);
        return Results.Ok(comprobante);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error al generar datos para comprobante: {request.CodigoReclamo}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Generar PDF de comprobante
app.MapPost("/api/entrega/generar-pdf-comprobante", [Authorize(Roles = "Personal de Entrega")] async (
    ComprobanteEntregaDTO comprobante,
    IEntregaService entregaService,
    HttpContext httpContext) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation($"Generando PDF para comprobante: {comprobante.CodigoReclamo}");

        var rutaPdf = await entregaService.GenerarPdfComprobanteAsync(comprobante);
        return Results.Ok(new { rutaPdf });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error al generar PDF para comprobante: {comprobante.CodigoReclamo}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Subir comprobante firmado
app.MapPost("/api/entrega/subir-comprobante", [Authorize(Roles = "Personal de Entrega")] async (
    SubirComprobanteRequest request,
    IEntregaService entregaService,
    HttpContext httpContext) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        var personalEntregaId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        logger.LogInformation($"Subiendo comprobante firmado para: {request.CodigoReclamo}");

        var resultado = await entregaService.SubirComprobanteAsync(request, personalEntregaId);
        return resultado ?
            Results.Ok(new { message = "Comprobante subido exitosamente" }) :
            Results.BadRequest(new { message = "No se pudo subir el comprobante" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error al subir comprobante: {request.CodigoReclamo}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Confirmar entrega
app.MapPost("/api/entrega/confirmar-entrega", [Authorize(Roles = "Personal de Entrega")] async (
    ConfirmarEntregaRequest request,
    IEntregaService entregaService,
    HttpContext httpContext) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        var personalEntregaId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        logger.LogInformation($"Confirmando entrega para: {request.CodigoReclamo}");

        var resultado = await entregaService.ConfirmarEntregaAsync(request, personalEntregaId);
        return resultado ?
            Results.Ok(new { message = "Entrega confirmada exitosamente" }) :
            Results.BadRequest(new { message = "No se pudo confirmar la entrega" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error al confirmar entrega: {request.CodigoReclamo}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

app.Run();