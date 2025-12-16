using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Usuarios
{
    public class Usuario
    {
        public int Id { get; private set; }
        public string Correo { get; private set; }
        public RolUsuario Rol { get; private set; }

        protected Usuario() { }

        public Usuario(string Correo, RolUsuario Rol)
        {
            this.Id = 0;
            this.Correo = Correo;
            this.Rol = Rol;
        }
    }
}
