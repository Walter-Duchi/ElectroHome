using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public int Id { get; set; }
        public string Correo { get; set; } = null!;
        public string Rol { get; set; } = null!;
    }
}
