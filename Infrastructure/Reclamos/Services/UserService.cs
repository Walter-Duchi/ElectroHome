using Application.DTOs.Auth;
using Application.DTOs.Reclamos.User;
using Application.DTOs.User;
using Infrastructure.Data;
using Infrastructure.Models;
using Infrastructure.Reclamos.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Reclamos.Services
{
    public class UserService : IUserService
    {
        private readonly ReclamosContext _context;

        public UserService(ReclamosContext context)
        {
            _context = context;
        }

        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request, int? creadoPorId)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo))
                throw new ArgumentException("El correo ya está registrado");

            if (await _context.Usuarios.AnyAsync(u => u.Identificacion == request.Identificacion))
                throw new ArgumentException("La identificación ya está registrada");

            if (!string.IsNullOrEmpty(request.RUC) && await _context.Usuarios.AnyAsync(u => u.Ruc == request.RUC))
                throw new ArgumentException("El RUC ya está registrado");

            if (request.TipoIdentificacion == "Cedula" && !ValidarCedulaEcuatoriana(request.Identificacion))
                throw new ArgumentException("La cédula ecuatoriana no es válida");

            var contrasenaGenerada = GenerarContrasenaSegura();

            var contrasenaHash = HashPassword(contrasenaGenerada);

            var usuario = new Usuario
            {
                Nombres = request.Nombres,
                Apellidos = request.Apellidos,
                RazonSocial = request.RazonSocial,
                TipoIdentificacion = request.TipoIdentificacion,
                Identificacion = request.Identificacion,
                Ruc = request.RUC,
                Correo = request.Correo,
                Contrasena = contrasenaHash,
                Celular = request.Celular,
                Convencional = request.Convencional,
                Pais = "Ecuador",
                DivisionAdministrativa = "Guayas",
                Ciudad = request.Ciudad,
                CodigoPostal = request.CodigoPostal,
                Direccion = request.Direccion,
                Rol = request.Rol,
                FechaCreacion = DateTime.UtcNow,
                NumCuentaBancaria = request.NumCuentaBancaria,
                TipoCuentaBancaria = request.TipoCuentaBancaria,
                Activo = true,
                ContribuyenteEspecial = request.ContribuyenteEspecial,
                ObligadoContabilidad = request.ObligadoContabilidad,
                CreadoPor = creadoPorId
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return new CreateUserResponse
            {
                Id = usuario.Id,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Correo = usuario.Correo,
                Celular = usuario.Celular,
                Rol = usuario.Rol,
                FechaCreacion = usuario.FechaCreacion,
                ContrasenaGenerada = contrasenaGenerada,
                Mensaje = "Usuario creado exitosamente. Guarde esta contraseña para proporcionarla al empleado."
            };
        }

        public async Task<CreateUserResponse> RegisterClientAsync(RegisterRequest request)
        {
            // Validar que el correo no exista
            if (await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo))
                throw new ArgumentException("El correo ya está registrado");

            // Validar que la identificación no exista
            if (await _context.Usuarios.AnyAsync(u => u.Identificacion == request.Identificacion))
                throw new ArgumentException("La identificación ya está registrada");

            // Validar RUC si se proporciona
            if (!string.IsNullOrEmpty(request.Ruc) && await _context.Usuarios.AnyAsync(u => u.Ruc == request.Ruc))
                throw new ArgumentException("El RUC ya está registrado");

            // Validar cédula ecuatoriana si el tipo es Cédula
            if (request.TipoIdentificacion == "Cedula" && !ValidarCedulaEcuatoriana(request.Identificacion))
                throw new ArgumentException("La cédula ecuatoriana no es válida");

            // Hashear la contraseña proporcionada
            var contrasenaHash = HashPassword(request.Contrasena);

            var usuario = new Usuario
            {
                Nombres = request.Nombres,
                Apellidos = request.Apellidos,
                RazonSocial = null,
                TipoIdentificacion = request.TipoIdentificacion,
                Identificacion = request.Identificacion,
                Ruc = request.Ruc,
                Correo = request.Correo,
                Contrasena = contrasenaHash,
                Celular = request.Celular,
                Convencional = request.Convencional,
                Pais = "Ecuador",
                DivisionAdministrativa = "Guayas",
                Ciudad = request.Ciudad,
                CodigoPostal = request.CodigoPostal,
                Direccion = request.Direccion,
                Rol = "Cliente",
                FechaCreacion = DateTime.UtcNow,
                NumCuentaBancaria = "0000000000",
                TipoCuentaBancaria = "Ahorro",
                Activo = true,
                ContribuyenteEspecial = false,
                ObligadoContabilidad = false,
                CreadoPor = null
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return new CreateUserResponse
            {
                Id = usuario.Id,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Correo = usuario.Correo,
                Celular = usuario.Celular,
                Rol = usuario.Rol,
                FechaCreacion = usuario.FechaCreacion,
                ContrasenaGenerada = request.Contrasena,
                Mensaje = "Cliente registrado exitosamente"
            };
        }

        private bool ValidarCedulaEcuatoriana(string cedula)
        {
            if (cedula.Length != 10 || !cedula.All(char.IsDigit))
                return false;

            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int total = 0;

            for (int i = 0; i < 9; i++)
            {
                int valor = int.Parse(cedula[i].ToString()) * coeficientes[i];
                if (valor >= 10)
                    valor -= 9;
                total += valor;
            }

            int digitoVerificador = total % 10;
            if (digitoVerificador != 0)
                digitoVerificador = 10 - digitoVerificador;

            return digitoVerificador == int.Parse(cedula[9].ToString());
        }

        private string GenerarContrasenaSegura()
        {
            const string mayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string minusculas = "abcdefghijklmnopqrstuvwxyz";
            const string numeros = "0123456789";
            const string especiales = "@$!%*?&";
            const string todosCaracteres = mayusculas + minusculas + numeros + especiales;

            var random = new Random();
            var passwordChars = new char[12];

            passwordChars[0] = mayusculas[random.Next(mayusculas.Length)];
            passwordChars[1] = minusculas[random.Next(minusculas.Length)];
            passwordChars[2] = numeros[random.Next(numeros.Length)];
            passwordChars[3] = especiales[random.Next(especiales.Length)];

            for (int i = 4; i < 12; i++)
            {
                passwordChars[i] = todosCaracteres[random.Next(todosCaracteres.Length)];
            }

            for (int i = 0; i < 12; i++)
            {
                int randomIndex = random.Next(i, 12);
                (passwordChars[i], passwordChars[randomIndex]) = (passwordChars[randomIndex], passwordChars[i]);
            }

            return new string(passwordChars);
        }

        private byte[] HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public Task<bool> CanCreateRole(string creatorRole, string targetRole)
        {
            if (creatorRole != "Administrador")
                return Task.FromResult(false);

            var rolesPermitidos = new List<string>
            {
                "Revisor", "Tecnico", "Personal de Entrega",
                "Analista_Datos", "Encargado_Inventario", "Gestor_Productos", "Administrador"
            };

            return Task.FromResult(rolesPermitidos.Contains(targetRole));
        }

        public async Task<ProfileResponse> GetProfileAsync(int userId)
        {
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado");

            return new ProfileResponse
            {
                Id = usuario.Id,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                RazonSocial = usuario.RazonSocial,
                TipoIdentificacion = usuario.TipoIdentificacion,
                Identificacion = usuario.Identificacion,
                Ruc = usuario.Ruc,
                Correo = usuario.Correo,
                Celular = usuario.Celular,
                Convencional = usuario.Convencional,
                Pais = usuario.Pais,
                DivisionAdministrativa = usuario.DivisionAdministrativa,
                Ciudad = usuario.Ciudad,
                CodigoPostal = usuario.CodigoPostal,
                Direccion = usuario.Direccion,
                Rol = usuario.Rol,
                FechaCreacion = usuario.FechaCreacion,
                NumCuentaBancaria = usuario.NumCuentaBancaria,
                TipoCuentaBancaria = usuario.TipoCuentaBancaria,
                ContribuyenteEspecial = usuario.ContribuyenteEspecial,
                ObligadoContabilidad = usuario.ObligadoContabilidad,
                Activo = usuario.Activo
            };
        }

        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado");

            if (!string.IsNullOrEmpty(request.Correo) && request.Correo != usuario.Correo)
            {
                var existe = await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo && u.Id != userId);
                if (existe)
                    throw new ArgumentException("El correo ya está registrado por otro usuario");

                if (!IsValidEmail(request.Correo))
                    throw new ArgumentException("Formato de correo inválido");

                usuario.Correo = request.Correo;
            }

            if (!string.IsNullOrEmpty(request.Celular))
            {
                if (!IsValidCelular(request.Celular))
                    throw new ArgumentException("Formato de celular inválido (debe ser 09XXXXXXXX)");
                usuario.Celular = request.Celular;
            }

            usuario.Convencional = string.IsNullOrEmpty(request.Convencional) ? null : request.Convencional;

            if (!string.IsNullOrEmpty(request.Ciudad))
                usuario.Ciudad = request.Ciudad;

            if (!string.IsNullOrEmpty(request.CodigoPostal))
            {
                if (!IsValidPostalCode(request.CodigoPostal))
                    throw new ArgumentException("Código postal inválido (6 dígitos)");
                usuario.CodigoPostal = request.CodigoPostal;
            }

            if (!string.IsNullOrEmpty(request.Direccion))
                usuario.Direccion = request.Direccion;

            if (!string.IsNullOrEmpty(request.NewPassword) || !string.IsNullOrEmpty(request.CurrentPassword))
            {
                if (string.IsNullOrEmpty(request.CurrentPassword))
                    throw new ArgumentException("Debe proporcionar la contraseña actual para cambiarla");

                if (string.IsNullOrEmpty(request.NewPassword))
                    throw new ArgumentException("Debe proporcionar la nueva contraseña");

                if (request.NewPassword != request.ConfirmNewPassword)
                    throw new ArgumentException("Las nuevas contraseñas no coinciden");

                var currentHash = HashPassword(request.CurrentPassword);
                if (!currentHash.SequenceEqual(usuario.Contrasena))
                    throw new ArgumentException("La contraseña actual es incorrecta");

                if (request.NewPassword.Length < 6)
                    throw new ArgumentException("La nueva contraseña debe tener al menos 6 caracteres");

                usuario.Contrasena = HashPassword(request.NewPassword);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidCelular(string celular)
        {
            var cleaned = System.Text.RegularExpressions.Regex.Replace(celular, @"\D", "");
            return cleaned.Length == 10 && cleaned.StartsWith("09");
        }

        private bool IsValidPostalCode(string codigoPostal)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(codigoPostal, @"^\d{6}$");
        }
    }
}