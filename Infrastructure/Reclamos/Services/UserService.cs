using Application.DTOs.Reclamos.User;
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

        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request, int creadoPorId)
        {
            // Validar que el correo no exista
            if (await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo))
                throw new ArgumentException("El correo ya está registrado");

            // Validar que la identificación no exista
            if (await _context.Usuarios.AnyAsync(u => u.Identificacion == request.Identificacion))
                throw new ArgumentException("La identificación ya está registrada");

            // Validar RUC si se proporciona
            if (!string.IsNullOrEmpty(request.RUC) && await _context.Usuarios.AnyAsync(u => u.Ruc == request.RUC))
                throw new ArgumentException("El RUC ya está registrado");

            // Validar cédula ecuatoriana si el tipo es Cédula
            if (request.TipoIdentificacion == "Cedula" && !ValidarCedulaEcuatoriana(request.Identificacion))
                throw new ArgumentException("La cédula ecuatoriana no es válida");

            // Generar contraseña aleatoria segura
            var contrasenaGenerada = GenerarContrasenaSegura();

            // Hashear la contraseña
            var contrasenaHash = HashPassword(contrasenaGenerada);

            // Crear el usuario
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

            // Asegurar al menos un carácter de cada tipo
            passwordChars[0] = mayusculas[random.Next(mayusculas.Length)];
            passwordChars[1] = minusculas[random.Next(minusculas.Length)];
            passwordChars[2] = numeros[random.Next(numeros.Length)];
            passwordChars[3] = especiales[random.Next(especiales.Length)];

            // Completar el resto de la contraseña
            for (int i = 4; i < 12; i++)
            {
                passwordChars[i] = todosCaracteres[random.Next(todosCaracteres.Length)];
            }

            // Mezclar los caracteres
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
            // Solo el administrador puede crear usuarios
            if (creatorRole != "Administrador")
                return Task.FromResult(false);

            // Roles que el administrador puede crear
            var rolesPermitidos = new List<string>
            {
                "Revisor", "Tecnico", "Personal de Entrega", "Vendedor",
                "Analista_Datos", "Encargado_Inventario", "Gestor_Productos", "Administrador"
            };

            return Task.FromResult(rolesPermitidos.Contains(targetRole));
        }
    }
}