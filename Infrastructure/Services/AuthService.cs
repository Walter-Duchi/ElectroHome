using Application.DTOs.Auth;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ReclamosContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(ReclamosContext context, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
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
    }
}
