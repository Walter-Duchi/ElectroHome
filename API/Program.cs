using Application.DTOs.Admin;
using Application.DTOs.Auth;
using Application.DTOs.Ecommerce;
using Application.DTOs.Inventario;
using Application.DTOs.Productos;
using Application.DTOs.Reclamos.Cliente;
using Application.DTOs.Reclamos.Entrega;
using Application.DTOs.Reclamos.Reclamo;
using Application.DTOs.Reclamos.Tecnico;
using Application.DTOs.Reclamos.User;
using Infrastructure.Data;
using Infrastructure.Facturacion.Services;
using Infrastructure.Facturacion.Services;
using Infrastructure.Payphone.DTOs;
using Infrastructure.Payphone.Services;
using Infrastructure.Reclamos.Interfaces;
using Infrastructure.Reclamos.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;
using Yamgooo.SRI.Sign;
using Yamgooo.SRI.Sign.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Configuración de logging EXTENDIDO
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
QuestPDF.Settings.License = LicenseType.Community;


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
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IDatosEmpresaService, DatosEmpresaService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IFirmaElectronicaService, FirmaElectronicaService>();
builder.Services.AddScoped<ISriFacturacionService, SriFacturacionService>();
builder.Services.AddScoped<IFacturacionService, FacturacionService>();
builder.Services.AddScoped<IPayphoneService, PayphoneService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IProductManagementService, ProductManagementService>();
builder.Services.AddScoped<IAnalistaService, AnalistaService>();
builder.Services.AddSriSignService(builder.Configuration, "SriSign");
builder.Services.AddHttpClient();


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

builder.Services.AddSwaggerGen();
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
    app.UseSwagger();
    app.UseSwaggerUI();
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

// Servir archivos estáticos desde la carpeta Documents
var documentsPath = Path.Combine(Directory.GetCurrentDirectory(), "Documents");
if (!Directory.Exists(documentsPath))
{
    Directory.CreateDirectory(documentsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(documentsPath),
    RequestPath = "/Documents",
    ServeUnknownFileTypes = true, // Para servir archivos PDF
    DefaultContentType = "application/octet-stream"
});

// También servir archivos desde wwwroot si los hay
app.UseStaticFiles();

// Configurar MIME types para archivos comunes
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".pdf"] = "application/pdf";
provider.Mappings[".txt"] = "text/plain";
provider.Mappings[".csv"] = "text/csv";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(documentsPath),
    RequestPath = "/Documents",
    ContentTypeProvider = provider
});

// Asegurar que la carpeta 'entrega' existe
var entregaPath = Path.Combine(documentsPath, "entrega");
if (!Directory.Exists(entregaPath))
{
    Directory.CreateDirectory(entregaPath);
}

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

//CREAR USUARIO COMO ADMINISTRADOR
app.MapGet("/api/admin/roles-permitidos", [Authorize(Roles = "Administrador")] () =>
{
    var rolesPermitidos = new List<string>
    {
        "Revisor", "Tecnico", "Personal de Entrega", "Vendedor",
        "Analista_Datos", "Encargado_Inventario", "Gestor_Productos", "Administrador"
    };

    return Results.Ok(rolesPermitidos);
});

app.MapPost("/api/admin/crear-usuario", [Authorize(Roles = "Administrador")] async (
    CreateUserRequest request,
    IUserService userService,
    HttpContext httpContext) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("=== INICIO: Crear Usuario (Administrador) ===");
        logger.LogInformation($"Solicitud: {System.Text.Json.JsonSerializer.Serialize(request)}");

        // Obtener ID del administrador que crea el usuario
        var administradorId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        logger.LogInformation($"Administrador ID: {administradorId} creando usuario");

        // Crear el usuario
        var response = await userService.CreateUserAsync(request, administradorId);

        logger.LogInformation("=== ÉXITO: Usuario creado ===");
        return Results.Ok(response);
    }
    catch (ArgumentException ex)
    {
        logger.LogWarning($"Error de validación: {ex.Message}");
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "=== ERROR: Crear Usuario ===");
        return Results.Problem(
            detail: $"Error interno: {ex.Message}",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Error del servidor");
    }
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
// ENDPOINTS PARA PERSONAL DE ENTREGA (ACTUALIZADOS)
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
        return Results.Ok(response);
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
        logger.LogInformation("=== INICIO: Seleccionar Reemplazo ===");
        logger.LogInformation($"Request: ReclamoProductoSnId={request.ReclamoProductoSnId}, NumeroSerieReemplazo={request.NumeroSerieReemplazo}");

        var personalEntregaId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        logger.LogInformation($"Personal Entrega ID: {personalEntregaId}");

        var resultado = await entregaService.SeleccionarReemplazoAsync(request, personalEntregaId);

        logger.LogInformation("=== ÉXITO: Reemplazo seleccionado ===");
        return resultado ?
            Results.Ok(new { message = "Reemplazo seleccionado exitosamente" }) :
            Results.BadRequest(new { message = "No se pudo seleccionar el reemplazo" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "=== ERROR: Seleccionar Reemplazo ===");
        return Results.Problem(
            detail: $"Error interno: {ex.Message}",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Error del servidor");
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

// Endpoint para servir PDFs de entrega
app.MapGet("/Documents/entrega/{fileName}", async (string fileName, HttpContext context) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Documents", "entrega", fileName);

    if (!File.Exists(filePath))
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Archivo no encontrado");
        return;
    }

    context.Response.ContentType = "application/pdf";
    await context.Response.SendFileAsync(filePath);
});

// ============================================
// ENDPOINTS PARA CLIENTE DASHBOARD
// ============================================

// Obtener dashboard del cliente
app.MapPost("/api/cliente/dashboard", [Authorize(Roles = "Cliente")] async (
    ClienteDashboardRequest request,
    IClienteService clienteService,
    HttpContext httpContext) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        var clienteId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        logger.LogInformation($"Obteniendo dashboard para cliente ID: {clienteId}");

        var dashboard = await clienteService.ObtenerDashboardClienteAsync(clienteId, request);
        return Results.Ok(dashboard);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al obtener dashboard del cliente");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Obtener PDF en base64
app.MapGet("/api/cliente/pdf/{tipo}/{nombreArchivo}", [Authorize(Roles = "Cliente")] async (
    string tipo,
    string nombreArchivo,
    IClienteService clienteService,
    HttpContext httpContext) =>
{
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        // Validar que el cliente tiene acceso a este PDF
        var clienteId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Construir ruta según tipo
        var rutaBase = Path.Combine(Directory.GetCurrentDirectory(), "Documents");
        string rutaPdf;

        if (tipo == "tecnico")
            rutaPdf = Path.Combine(rutaBase, "reclamos", nombreArchivo);
        else if (tipo == "entrega")
            rutaPdf = Path.Combine(rutaBase, "entrega", nombreArchivo);
        else
            return Results.BadRequest("Tipo de PDF no válido");

        logger.LogInformation($"Solicitando PDF: {rutaPdf} para cliente ID: {clienteId}");

        // Verificar que el archivo existe
        if (!File.Exists(rutaPdf))
        {
            logger.LogWarning($"PDF no encontrado: {rutaPdf}");
            return Results.NotFound("Archivo no encontrado");
        }

        // Obtener PDF en base64
        var pdfBase64 = await clienteService.ObtenerPdfBase64Async(rutaPdf);

        if (string.IsNullOrEmpty(pdfBase64))
            return Results.NotFound("No se pudo leer el archivo");

        return Results.Ok(new { pdfBase64, nombreArchivo });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error al obtener PDF: {tipo}/{nombreArchivo}");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// ============================================
// ENDPOINTS PARA ADMIN - DATOS EMPRESA
// ============================================

// Obtener datos de la empresa
app.MapGet("/api/admin/datos-empresa", [Authorize(Roles = "Administrador")] async (IDatosEmpresaService datosEmpresaService, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Obteniendo datos de la empresa...");
        var datos = await datosEmpresaService.ObtenerDatosEmpresaAsync();
        return datos != null ? Results.Ok(datos) : Results.NotFound(new { message = "No se han configurado los datos de la empresa." });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al obtener datos de la empresa.");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Actualizar datos de la empresa
app.MapPut("/api/admin/datos-empresa", [Authorize(Roles = "Administrador")] async (UpdateDatosEmpresaRequest request, IDatosEmpresaService datosEmpresaService, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Actualizando datos de la empresa...");
        var resultado = await datosEmpresaService.ActualizarDatosEmpresaAsync(request);
        return Results.Ok(resultado);
    }
    catch (ArgumentException ex)
    {
        logger.LogWarning(ex, "Error de validación al actualizar datos de la empresa.");
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al actualizar datos de la empresa.");
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// ============================================
// ENDPOINTS PARA E-COMMERCE (PÚBLICOS Y AUTENTICADOS)
// ============================================

// Categorías
app.MapGet("/api/ecommerce/categorias", async (ICategoryService categoryService) =>
{
    var categorias = await categoryService.GetAllCategoriesAsync();
    return Results.Ok(categorias);
}).AllowAnonymous();

// Productos con filtros
app.MapPost("/api/ecommerce/productos", async (ProductFilterRequest filter, IProductService productService) =>
{
    var productos = await productService.GetProductsAsync(filter);
    return Results.Ok(productos);
}).AllowAnonymous();

// Producto por ID
app.MapGet("/api/ecommerce/productos/{id:int}", async (int id, IProductService productService) =>
{
    var producto = await productService.GetProductByIdAsync(id);
    return producto is not null ? Results.Ok(producto) : Results.NotFound();
}).AllowAnonymous();

// Productos populares
app.MapGet("/api/ecommerce/productos/populares", async (IProductService productService) =>
{
    var productos = await productService.GetPopularProductsAsync(10);
    return Results.Ok(productos);
}).AllowAnonymous();

// Nuevos productos
app.MapGet("/api/ecommerce/productos/nuevos", async (IProductService productService) =>
{
    var productos = await productService.GetNewArrivalsAsync(10);
    return Results.Ok(productos);
}).AllowAnonymous();

// Carrito (requiere autenticación)
app.MapGet("/api/ecommerce/carrito", [Authorize] async (HttpContext httpContext, ICartService cartService) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    var carrito = await cartService.GetCartAsync(usuarioId);
    return Results.Ok(carrito);
});

app.MapPost("/api/ecommerce/carrito", [Authorize] async (AddToCartRequest request, HttpContext httpContext, ICartService cartService) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    var success = await cartService.AddToCartAsync(usuarioId, request);
    return success ? Results.Ok() : Results.BadRequest("No se pudo agregar al carrito");
});

app.MapPut("/api/ecommerce/carrito/{productoId:int}", [Authorize] async (int productoId, int cantidad, HttpContext httpContext, ICartService cartService) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    var success = await cartService.UpdateCartItemQuantityAsync(usuarioId, productoId, cantidad);
    return success ? Results.Ok() : Results.BadRequest();
});

app.MapDelete("/api/ecommerce/carrito/{productoId:int}", [Authorize] async (int productoId, HttpContext httpContext, ICartService cartService) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    var success = await cartService.RemoveFromCartAsync(usuarioId, productoId);
    return success ? Results.Ok() : Results.BadRequest();
});

app.MapDelete("/api/ecommerce/carrito", [Authorize] async (HttpContext httpContext, ICartService cartService) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    var success = await cartService.ClearCartAsync(usuarioId);
    return success ? Results.Ok() : Results.BadRequest();
});

// ============================================
// ENDPOINT DE FACTURACIÓN ELECTRÓNICA
// ============================================
app.MapPost("/api/facturacion/facturar/{ventaId:int}", async (int ventaId, IFacturacionService facturacionService) =>
{
    try
    {
        await facturacionService.FacturarVenta(ventaId);
        return Results.Ok(new { mensaje = "Proceso de facturación iniciado" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});
// .RequireAuthorization(); // Descomenta si quieres requerir autenticación

// Endpoint para consultar autorización de una clave de acceso
app.MapGet("/api/facturacion/consultar/{claveAcceso}", async (string claveAcceso, ISriFacturacionService sriService, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation($"Consultando autorización para clave: {claveAcceso}");
        var respuesta = await sriService.ConsultarAutorizacion(claveAcceso);

        // Intentar obtener autorización desde el objeto deserializado
        if (respuesta.RespuestaDeserializada?.RespuestaAutorizacionComprobante?.autorizaciones?.Length > 0)
        {
            var autorizacion = respuesta.RespuestaDeserializada.RespuestaAutorizacionComprobante.autorizaciones[0];
            logger.LogInformation("Autorización encontrada por deserialización: Estado={Estado}, Numero={Numero}",
                autorizacion.estado, autorizacion.numeroAutorizacion);
            return Results.Ok(new
            {
                claveAcceso,
                estado = autorizacion.estado,
                fechaAutorizacion = autorizacion.fechaAutorizacion,
                numeroAutorizacion = autorizacion.numeroAutorizacion,
                ambiente = autorizacion.ambiente,
                comprobante = autorizacion.comprobante
            });
        }

        // Si no se pudo por deserialización, intentar parseo manual del XML crudo
        string xmlParaParsear = respuesta.XmlRespuestaCruda;
        if (!string.IsNullOrEmpty(xmlParaParsear))
        {
            try
            {
                var doc = XDocument.Parse(xmlParaParsear);
                // Buscar elemento <autorizacion> por nombre local (sin namespace)
                var authElement = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "autorizacion");

                if (authElement != null)
                {
                    var estado = (string)authElement.Element(XName.Get("estado"));
                    var numero = (string)authElement.Element(XName.Get("numeroAutorizacion"));
                    var fechaStr = (string)authElement.Element(XName.Get("fechaAutorizacion"));
                    var ambiente = (string)authElement.Element(XName.Get("ambiente"));
                    var comprobanteXml = (string)authElement.Element(XName.Get("comprobante"));

                    logger.LogInformation("Autorización encontrada por parseo manual: Estado={Estado}, Numero={Numero}", estado, numero);
                    return Results.Ok(new
                    {
                        claveAcceso,
                        estado,
                        fechaAutorizacion = fechaStr,
                        numeroAutorizacion = numero,
                        ambiente,
                        comprobante = comprobanteXml
                    });
                }
                else
                {
                    logger.LogWarning("No se encontró elemento <autorizacion> en el XML crudo.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al parsear manualmente el XML crudo de autorización.");
            }
        }

        // Si aún no se encontró, devolver mensaje por defecto
        logger.LogWarning("No se encontraron autorizaciones en la respuesta deserializada ni en parseo manual.");
        return Results.Ok(new
        {
            claveAcceso,
            estado = "NO PROCESADO AÚN",
            mensaje = "El comprobante aún no ha sido procesado o no existe",
            xmlDepuracion = respuesta.XmlRespuestaCruda // opcional, útil para debug
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al consultar autorización");
        return Results.Problem($"Error interno: {ex.Message}");
    }
}).AllowAnonymous();

// ============================================
// ENDPOINTS DE PAYPHONE (PASARELA DE PAGO)
// ============================================

// Inicializar transacción (requiere autenticación)
app.MapPost("/api/payphone/init", [Authorize] async (HttpContext httpContext, IPayphoneService payphoneService) =>
{
    try
    {
        var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var response = await payphoneService.InitializeTransactionAsync(usuarioId);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Confirmar transacción (requiere autenticación)
app.MapPost("/api/payphone/confirm", [Authorize] async (PayphoneConfirmRequest request, HttpContext httpContext, IPayphoneService payphoneService) =>
{
    try
    {
        var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var response = await payphoneService.ConfirmTransactionAsync(request, usuarioId);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Endpoint para descargar PDF de factura
app.MapGet("/api/facturacion/pdf/{ventaId:int}", [Authorize] async (int ventaId, HttpContext httpContext, ReclamosContext context, IConfiguration config) =>
{
    var venta = await context.Ventas.FindAsync(ventaId);
    if (venta == null)
        return Results.NotFound();

    // Verificar que el usuario es el comprador o tiene permisos
    // (por simplicidad, solo verificamos que el usuario autenticado es el cliente)
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    if (venta.FkEmpresaCliente != usuarioId)
        return Results.Forbid();

    if (string.IsNullOrEmpty(venta.PdfPath) || !File.Exists(venta.PdfPath))
        return Results.NotFound("Factura PDF no disponible");

    var bytes = await File.ReadAllBytesAsync(venta.PdfPath);
    return Results.File(bytes, "application/pdf", $"factura_{venta.CodigoFactura}.pdf");
});

// ============================================
// ENDPOINTS PARA ENCARGADO DE INVENTARIO
// ============================================

// Ubicaciones
app.MapGet("/api/inventario/ubicaciones", [Authorize(Roles = "Encargado_Inventario")] async (IInventoryService service) =>
{
    var ubicaciones = await service.GetAllUbicacionesAsync();
    return Results.Ok(ubicaciones);
});

app.MapGet("/api/inventario/ubicaciones/{id:int}", [Authorize(Roles = "Encargado_Inventario")] async (int id, IInventoryService service) =>
{
    var ubicacion = await service.GetUbicacionByIdAsync(id);
    return ubicacion is not null ? Results.Ok(ubicacion) : Results.NotFound();
});

app.MapPost("/api/inventario/ubicaciones", [Authorize(Roles = "Encargado_Inventario")] async (CreateUbicacionRequest request, IInventoryService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var ubicacion = await service.CreateUbicacionAsync(request, usuarioId);
        return Results.Created($"/api/inventario/ubicaciones/{ubicacion.Id}", ubicacion);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPut("/api/inventario/ubicaciones", [Authorize(Roles = "Encargado_Inventario")] async (UpdateUbicacionRequest request, IInventoryService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var ubicacion = await service.UpdateUbicacionAsync(request, usuarioId);
        return Results.Ok(ubicacion);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapDelete("/api/inventario/ubicaciones/{id:int}", [Authorize(Roles = "Encargado_Inventario")] async (int id, IInventoryService service) =>
{
    try
    {
        var result = await service.DeleteUbicacionAsync(id);
        return result ? Results.Ok() : Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

// Movimientos
app.MapGet("/api/inventario/movimientos", [Authorize(Roles = "Encargado_Inventario")] async (int? productoId, DateTime? desde, DateTime? hasta, IInventoryService service) =>
{
    var movimientos = await service.GetMovimientosAsync(productoId, desde, hasta);
    return Results.Ok(movimientos);
});

app.MapPost("/api/inventario/entrada", [Authorize(Roles = "Encargado_Inventario")] async (CreateMovimientoRequest request, IInventoryService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var movimiento = await service.RegistrarEntradaAsync(request, usuarioId);
        return Results.Ok(movimiento);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPost("/api/inventario/salida", [Authorize(Roles = "Encargado_Inventario")] async (CreateMovimientoRequest request, IInventoryService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var movimiento = await service.RegistrarSalidaAsync(request, usuarioId);
        return Results.Ok(movimiento);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPost("/api/inventario/ajuste", [Authorize(Roles = "Encargado_Inventario")] async (CreateMovimientoRequest request, IInventoryService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var movimiento = await service.RegistrarAjusteAsync(request, usuarioId);
        return Results.Ok(movimiento);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPost("/api/inventario/devolucion", [Authorize(Roles = "Encargado_Inventario")] async (CreateMovimientoRequest request, IInventoryService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var movimiento = await service.RegistrarDevolucionAsync(request, usuarioId);
        return Results.Ok(movimiento);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

// Números de serie
app.MapGet("/api/inventario/numeros-serie", [Authorize(Roles = "Encargado_Inventario")] async (int? productoId, string? estado, int? ubicacionId, IInventoryService service) =>
{
    var numeros = await service.GetNumerosSerieAsync(productoId, estado, ubicacionId);
    return Results.Ok(numeros);
});

app.MapGet("/api/inventario/numeros-serie/{numero}", [Authorize(Roles = "Encargado_Inventario")] async (string numero, IInventoryService service) =>
{
    var ns = await service.GetNumeroSerieByNumeroAsync(numero);
    return ns is not null ? Results.Ok(ns) : Results.NotFound();
});

app.MapPut("/api/inventario/numeros-serie", [Authorize(Roles = "Encargado_Inventario")] async (UpdateNumeroSerieRequest request, IInventoryService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    var result = await service.UpdateNumeroSerieAsync(request, usuarioId);
    return result ? Results.Ok() : Results.NotFound();
});

// Proveedores
app.MapGet("/api/inventario/proveedores", [Authorize(Roles = "Encargado_Inventario")] async (bool soloActivos, IInventoryService service) =>
{
    var proveedores = await service.GetAllProveedoresAsync(soloActivos);
    return Results.Ok(proveedores);
});

app.MapGet("/api/inventario/proveedores/{id:int}", [Authorize(Roles = "Encargado_Inventario")] async (int id, IInventoryService service) =>
{
    var proveedor = await service.GetProveedorByIdAsync(id);
    return proveedor is not null ? Results.Ok(proveedor) : Results.NotFound();
});

app.MapPost("/api/inventario/proveedores", [Authorize(Roles = "Encargado_Inventario")] async (CreateProveedorRequest request, IInventoryService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var proveedor = await service.CreateProveedorAsync(request, usuarioId);
        return Results.Created($"/api/inventario/proveedores/{proveedor.Id}", proveedor);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPut("/api/inventario/proveedores", [Authorize(Roles = "Encargado_Inventario")] async (UpdateProveedorRequest request, IInventoryService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var proveedor = await service.UpdateProveedorAsync(request, usuarioId);
        return Results.Ok(proveedor);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPatch("/api/inventario/proveedores/{id:int}/toggle", [Authorize(Roles = "Encargado_Inventario")] async (int id, bool activo, IInventoryService service) =>
{
    var result = await service.ToggleProveedorActivoAsync(id, activo);
    return result ? Results.Ok() : Results.NotFound();
});

// ============================================
// ENDPOINTS PARA GESTOR DE PRODUCTOS
// ============================================

// Productos (gestión completa)
app.MapGet("/api/productos/gestion", [Authorize(Roles = "Gestor_Productos,Encargado_Inventario")] async (bool includeInactivos, IProductManagementService service) =>
{
    var productos = await service.GetAllProductosAsync(includeInactivos);
    return Results.Ok(productos);
});

app.MapGet("/api/productos/gestion/{id:int}", [Authorize(Roles = "Gestor_Productos,Encargado_Inventario")] async (int id, IProductManagementService service) =>
{
    var producto = await service.GetProductoByIdAsync(id);
    return producto is not null ? Results.Ok(producto) : Results.NotFound();
});

app.MapPost("/api/productos/gestion", [Authorize(Roles = "Gestor_Productos")] async (HttpRequest request, IProductManagementService service, HttpContext httpContext, IWebHostEnvironment env) =>
{
    var form = await request.ReadFormAsync();
    var createRequest = new CreateProductoRequest
    {
        Sku = form["Sku"],
        Codigo = form["Codigo"],
        MarcaId = int.Parse(form["MarcaId"]),
        CategoriaId = string.IsNullOrEmpty(form["CategoriaId"]) ? null : int.Parse(form["CategoriaId"]),
        Modelo = form["Modelo"],
        Especificacion = form["Especificacion"],
        Descripcion = form["Descripcion"],
        Precio = decimal.Parse(form["Precio"]),
        DiasGarantia = int.Parse(form["DiasGarantia"]),
        Visibilidad = form["Visibilidad"],
        PesoKg = string.IsNullOrEmpty(form["PesoKg"]) ? null : decimal.Parse(form["PesoKg"]),
        AltoCm = decimal.Parse(form["AltoCm"]),
        AnchoCm = decimal.Parse(form["AnchoCm"]),
        ProfundidadCm = decimal.Parse(form["ProfundidadCm"]),
        ImagenPrincipal = form.Files.GetFile("ImagenPrincipal"),
        ImagenesAdicionales = form.Files.Where(f => f.Name == "ImagenesAdicionales").ToList()
    };

    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var producto = await service.CreateProductoAsync(createRequest, usuarioId, env.WebRootPath);
        return Results.Created($"/api/productos/gestion/{producto.Id}", producto);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPut("/api/productos/gestion", [Authorize(Roles = "Gestor_Productos")] async (HttpRequest request, IProductManagementService service, HttpContext httpContext, IWebHostEnvironment env) =>
{
    var form = await request.ReadFormAsync();
    var updateRequest = new UpdateProductoRequest
    {
        Id = int.Parse(form["Id"]),
        Sku = form["Sku"],
        Codigo = form["Codigo"],
        MarcaId = int.Parse(form["MarcaId"]),
        CategoriaId = string.IsNullOrEmpty(form["CategoriaId"]) ? null : int.Parse(form["CategoriaId"]),
        Modelo = form["Modelo"],
        Especificacion = form["Especificacion"],
        Descripcion = form["Descripcion"],
        Precio = decimal.Parse(form["Precio"]),
        DiasGarantia = int.Parse(form["DiasGarantia"]),
        Visibilidad = form["Visibilidad"],
        Activo = bool.Parse(form["Activo"]),
        PesoKg = string.IsNullOrEmpty(form["PesoKg"]) ? null : decimal.Parse(form["PesoKg"]),
        AltoCm = decimal.Parse(form["AltoCm"]),
        AnchoCm = decimal.Parse(form["AnchoCm"]),
        ProfundidadCm = decimal.Parse(form["ProfundidadCm"]),
        ImagenPrincipal = form.Files.GetFile("ImagenPrincipal"),
        ImagenesAdicionales = form.Files.Where(f => f.Name == "ImagenesAdicionales").ToList(),
        ImagenesAEliminar = form["ImagenesAEliminar"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
    };

    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var producto = await service.UpdateProductoAsync(updateRequest, usuarioId, env.WebRootPath);
        return Results.Ok(producto);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPatch("/api/productos/gestion/{id:int}/toggle", [Authorize(Roles = "Gestor_Productos")] async (int id, bool activo, IProductManagementService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    var result = await service.ToggleProductoActivoAsync(id, activo, usuarioId);
    return result ? Results.Ok() : Results.NotFound();
});

// Categorías (gestión)
app.MapGet("/api/productos/categorias", [Authorize(Roles = "Gestor_Productos,Encargado_Inventario")] async (bool includeInactivos, IProductManagementService service) =>
{
    var categorias = await service.GetAllCategoriasAsync(includeInactivos);
    return Results.Ok(categorias);
});

app.MapGet("/api/productos/categorias/{id:int}", [Authorize(Roles = "Gestor_Productos,Encargado_Inventario")] async (int id, IProductManagementService service) =>
{
    var categoria = await service.GetCategoriaByIdAsync(id);
    return categoria is not null ? Results.Ok(categoria) : Results.NotFound();
});

app.MapPost("/api/productos/categorias", [Authorize(Roles = "Gestor_Productos")] async (CreateCategoriaRequest request, IProductManagementService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var categoria = await service.CreateCategoriaAsync(request, usuarioId);
        return Results.Created($"/api/productos/categorias/{categoria.Id}", categoria);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPut("/api/productos/categorias", [Authorize(Roles = "Gestor_Productos")] async (UpdateCategoriaRequest request, IProductManagementService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var categoria = await service.UpdateCategoriaAsync(request, usuarioId);
        return Results.Ok(categoria);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapDelete("/api/productos/categorias/{id:int}", [Authorize(Roles = "Gestor_Productos")] async (int id, IProductManagementService service) =>
{
    try
    {
        var result = await service.DeleteCategoriaAsync(id);
        return result ? Results.Ok() : Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

// Marcas (gestión)
app.MapGet("/api/productos/marcas", [Authorize(Roles = "Gestor_Productos,Encargado_Inventario")] async (IProductManagementService service) =>
{
    var marcas = await service.GetAllMarcasAsync();
    return Results.Ok(marcas);
});

app.MapGet("/api/productos/marcas/{id:int}", [Authorize(Roles = "Gestor_Productos,Encargado_Inventario")] async (int id, IProductManagementService service) =>
{
    var marca = await service.GetMarcaByIdAsync(id);
    return marca is not null ? Results.Ok(marca) : Results.NotFound();
});

app.MapPost("/api/productos/marcas", [Authorize(Roles = "Gestor_Productos")] async (CreateMarcaRequest request, IProductManagementService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var marca = await service.CreateMarcaAsync(request, usuarioId);
        return Results.Created($"/api/productos/marcas/{marca.Id}", marca);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPut("/api/productos/marcas", [Authorize(Roles = "Gestor_Productos")] async (UpdateMarcaRequest request, IProductManagementService service, HttpContext httpContext) =>
{
    var usuarioId = int.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    try
    {
        var marca = await service.UpdateMarcaAsync(request, usuarioId);
        return Results.Ok(marca);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapDelete("/api/productos/marcas/{id:int}", [Authorize(Roles = "Gestor_Productos")] async (int id, IProductManagementService service) =>
{
    try
    {
        var result = await service.DeleteMarcaAsync(id);
        return result ? Results.Ok() : Results.NotFound();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

// ============================================
// ENDPOINTS PARA ANALISTA DE DATOS
// ============================================

app.MapGet("/api/analista/dashboard", [Authorize(Roles = "Analista_Datos")] async (IAnalistaService service) =>
{
    var dashboard = await service.ObtenerDashboardAsync();
    return Results.Ok(dashboard);
});

app.MapGet("/api/analista/exportar/ventas", [Authorize(Roles = "Analista_Datos")] async (DateTime? desde, DateTime? hasta, IAnalistaService service) =>
{
    var csv = await service.ExportarReporteVentasAsync(desde, hasta);
    return Results.File(csv, "text/csv", $"ventas_{DateTime.Now:yyyyMMdd}.csv");
});

app.MapGet("/api/analista/exportar/inventario", [Authorize(Roles = "Analista_Datos")] async (IAnalistaService service) =>
{
    var csv = await service.ExportarReporteInventarioAsync();
    return Results.File(csv, "text/csv", $"inventario_{DateTime.Now:yyyyMMdd}.csv");
});

app.Run();