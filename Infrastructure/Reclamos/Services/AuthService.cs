using Application.DTOs.Auth;
using Infrastructure.Data;
using Infrastructure.Models;
using Infrastructure.Reclamos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Reclamos.Services
{
    public class AuthService : IAuthService
    {
        private readonly ReclamosContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ReclamosContext context,
                          IJwtTokenGenerator jwtTokenGenerator,
                          IEmailService emailService,
                          IConfiguration configuration,
                          ILogger<AuthService> logger)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }


        public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
        {
            // Buscar usuario por correo
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == request.Correo);

            if (usuario == null)
                throw new UnauthorizedAccessException("Credenciales incorrectas");

            // Calcular hash SHA256 de la contraseña proporcionada
            using var sha256 = SHA256.Create();
            var inputBytes = Encoding.UTF8.GetBytes(request.Contrasena);
            var hashBytes = sha256.ComputeHash(inputBytes);

            // Comparar con el hash almacenado en la base de datos
            if (!hashBytes.SequenceEqual(usuario.Contrasena))
                throw new UnauthorizedAccessException("Credenciales incorrectas");

            // Generar token JWT
            var token = _jwtTokenGenerator.GenerateToken(
                usuario.Id,
                usuario.Correo,
                usuario.Rol
            );

            return new LoginResponse
            {
                Token = token,
                Id = usuario.Id,
                Correo = usuario.Correo,
                Rol = usuario.Rol
            };
        }

        public async Task<bool> RequestPasswordResetAsync(string correo)
        {
            try
            {
                _logger.LogInformation($"Iniciando solicitud de restablecimiento para: {correo}");

                // Buscar usuario por correo
                var usuario = await _context.Usuarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Correo == correo);

                // Por seguridad, siempre retornamos éxito aunque el correo no exista
                if (usuario == null)
                {
                    _logger.LogWarning($"Correo no encontrado: {correo} (se retorna éxito por seguridad)");
                    return true;
                }

                _logger.LogInformation($"Usuario encontrado: {usuario.Nombres} {usuario.Apellidos}");

                // Generar token único
                var token = GenerateSecureToken();
                _logger.LogDebug($"Token generado: {token.Substring(0, 10)}...");

                // Invalidar cualquier token previo del usuario
                await _context.TokensDeAccesos
                    .Where(t => t.FkUsuario == usuario.Id &&
                               t.TipoToken == "ResetPassword" &&
                               t.Vigente)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(t => t.Vigente, false));

                // Crear nuevo token
                var passwordResetToken = new TokensDeAcceso
                {
                    Token = token,
                    FechaCreacion = DateTime.UtcNow,
                    FechaExpiracion = DateTime.UtcNow.AddMinutes(15),
                    Vigente = true,
                    TipoToken = "ResetPassword",
                    FkUsuario = usuario.Id
                };

                _context.TokensDeAccesos.Add(passwordResetToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Token guardado en BD para usuario ID: {usuario.Id}");

                // Generar enlace de restablecimiento
                var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
                var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}";

                _logger.LogInformation($"Enlace generado: {resetLink}");

                // Enviar correo
                var emailResult = await _emailService.SendPasswordResetEmailAsync(
                    usuario.Correo,
                    resetLink,
                    usuario.Nombres
                );

                if (emailResult)
                {
                    _logger.LogInformation($"Correo enviado exitosamente a: {usuario.Correo}");
                }
                else
                {
                    _logger.LogError($"Falló el envío de correo a: {usuario.Correo}");
                }

                return emailResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en RequestPasswordResetAsync para {correo}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ValidateResetTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("Token vacío recibido");
                    return false;
                }

                _logger.LogInformation($"Validando token: {token.Substring(0, 10)}...");

                var tokenEntity = await _context.TokensDeAccesos
                    .Include(t => t.FkUsuarioNavigation)
                    .FirstOrDefaultAsync(t => t.Token == token &&
                                             t.TipoToken == "ResetPassword" &&
                                             t.Vigente);

                if (tokenEntity == null)
                {
                    _logger.LogWarning("Token no encontrado o ya usado");
                    return false;
                }

                // Verificar si el token ha expirado
                if (tokenEntity.FechaExpiracion < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Token expirado (expiración: {tokenEntity.FechaExpiracion})");
                    tokenEntity.Vigente = false;
                    await _context.SaveChangesAsync();
                    return false;
                }

                _logger.LogInformation($"Token válido para usuario: {tokenEntity.FkUsuarioNavigation?.Correo}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en ValidateResetTokenAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string nuevaContrasena)
        {
            try
            {
                _logger.LogInformation($"Iniciando restablecimiento con token: {token.Substring(0, 10)}...");

                var tokenEntity = await _context.TokensDeAccesos
                    .Include(t => t.FkUsuarioNavigation)
                    .FirstOrDefaultAsync(t => t.Token == token &&
                                             t.TipoToken == "ResetPassword" &&
                                             t.Vigente);

                if (tokenEntity == null)
                {
                    _logger.LogWarning("Token no encontrado o ya usado");
                    return false;
                }

                if (tokenEntity.FechaExpiracion < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Token expirado: {tokenEntity.FechaExpiracion}");
                    tokenEntity.Vigente = false;
                    await _context.SaveChangesAsync();
                    return false;
                }

                // Validar fuerza de la contraseña
                if (!IsStrongPassword(nuevaContrasena))
                {
                    _logger.LogWarning("Contraseña no cumple con los requisitos de seguridad");
                    throw new ArgumentException("La contraseña no cumple con los requisitos de seguridad");
                }

                // Calcular hash SHA256 de la nueva contraseña
                using var sha256 = SHA256.Create();
                var inputBytes = Encoding.UTF8.GetBytes(nuevaContrasena);
                var hashBytes = sha256.ComputeHash(inputBytes);

                // Actualizar contraseña del usuario
                tokenEntity.FkUsuarioNavigation.Contrasena = hashBytes;

                // Marcar token como usado
                tokenEntity.Vigente = false;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Contraseña restablecida exitosamente para usuario: {tokenEntity.FkUsuarioNavigation.Correo}");
                return true;
            }
            catch (ArgumentException)
            {
                throw; // Re-lanzar validación de contraseña
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en ResetPasswordAsync: {ex.Message}");
                return false;
            }
        }

        private string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[32];
            rng.GetBytes(tokenBytes);

            // Convertir a Base64 URL-safe
            return Convert.ToBase64String(tokenBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        private bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            var hasUpperCase = false;
            var hasLowerCase = false;
            var hasDigit = false;
            var hasSpecialChar = false;

            foreach (var c in password)
            {
                if (char.IsUpper(c)) hasUpperCase = true;
                else if (char.IsLower(c)) hasLowerCase = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if ("@$!%*?&".Contains(c)) hasSpecialChar = true;
            }

            return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }
    }
}
