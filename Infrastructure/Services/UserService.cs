using Application.DTOs.User;
using Infrastructure.Data;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ReclamosContext _context;
        private readonly IBankCardValidator _cardValidator;

        public UserService(ReclamosContext context, IBankCardValidator cardValidator)
        {
            _context = context;
            _cardValidator = cardValidator;
        }

        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request, string currentUserRole)
        {
            if (!CanCreateRole(currentUserRole, request.Rol))
                throw new UnauthorizedAccessException($"No tienes permiso para crear usuarios con rol {request.Rol}");

            if (await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo))
                throw new ArgumentException("El correo ya está registrado");

            if (await _context.Usuarios.AnyAsync(u => u.Ruc == request.RUC))
                throw new ArgumentException("El RUC ya está registrado");

            if (!IsValidRol(request.Rol))
                throw new ArgumentException($"Rol inválido: {request.Rol}");

            if (request.Rol == "Cliente")
            {
                if (string.IsNullOrWhiteSpace(request.NumCuentaBancaria))
                    throw new ArgumentException("El número de cuenta bancaria es obligatorio para clientes");

                if (!_cardValidator.ValidateCardNumber(request.NumCuentaBancaria))
                    throw new ArgumentException("El número de cuenta bancaria no es válido");
            }
            else
            {
                request.NumCuentaBancaria = null;
            }

            var usuario = new Usuario
            {
                Nombres = request.Nombres,
                Apellidos = request.Apellidos,
                Correo = request.Correo,
                Contrasena = HashPassword(request.Contrasena),
                Celular = request.Celular,
                Convencional = request.Convencional,
                Ruc = request.RUC,
                Rol = request.Rol,
                FechaCreacion = DateTime.UtcNow,
                NumCuentaBancaria = request.NumCuentaBancaria
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
                FechaCreacion = usuario.FechaCreacion
            };
        }

        public bool CanCreateRole(string creatorRole, string targetRole)
        {
            return creatorRole switch
            {
                "Revisor" => targetRole == "Cliente" || targetRole == "Revisor",
                "Tecnico" => targetRole == "Tecnico",
                "Personal de Entrega" => targetRole == "Personal de Entrega",
                _ => false
            };
        }

        private bool IsValidRol(string rol)
        {
            return rol switch
            {
                "Cliente" => true,
                "Revisor" => true,
                "Tecnico" => true,
                "Personal de Entrega" => true,
                _ => false
            };
        }

        private byte[] HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        Task<bool> IUserService.CanCreateRole(string creatorRole, string targetRole)
        {
            throw new NotImplementedException();
        }
    }
}