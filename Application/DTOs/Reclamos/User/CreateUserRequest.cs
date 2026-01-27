using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Reclamos.User
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Los nombres son obligatorios")]
        public string Nombres { get; set; } = null!;

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        public string Apellidos { get; set; } = null!;

        public string? RazonSocial { get; set; }

        [Required(ErrorMessage = "El tipo de identificación es obligatorio")]
        [RegularExpression("^(Cedula|Pasaporte)$", ErrorMessage = "Tipo de identificación inválido")]
        public string TipoIdentificacion { get; set; } = null!;

        [Required(ErrorMessage = "La identificación es obligatoria")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "La identificación debe tener entre 10 y 13 caracteres")]
        public string Identificacion { get; set; } = null!;

        public string? RUC { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "El celular es obligatorio")]
        [Phone(ErrorMessage = "Formato de celular inválido")]
        public string Celular { get; set; } = null!;

        public string? Convencional { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        public string Ciudad { get; set; } = "Guayaquil";

        [Required(ErrorMessage = "El código postal es obligatorio")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Código postal inválido para Guayaquil")]
        public string CodigoPostal { get; set; } = null!;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; } = null!;

        [Required(ErrorMessage = "El rol es obligatorio")]
        [RegularExpression("^(Revisor|Tecnico|Personal de Entrega|Vendedor|Analista_Datos|Encargado_Inventario|Gestor_Productos|Administrador)$",
            ErrorMessage = "Rol inválido. Los roles permitidos son: Revisor, Tecnico, Personal de Entrega, Vendedor, Analista_Datos, Encargado_Inventario, Gestor_Productos, Administrador")]
        public string Rol { get; set; } = null!;

        [Required(ErrorMessage = "El número de cuenta bancaria es obligatorio")]
        public string NumCuentaBancaria { get; set; } = null!;

        [Required(ErrorMessage = "El tipo de cuenta bancaria es obligatorio")]
        [RegularExpression("^(Ahorro|Corriente)$", ErrorMessage = "Tipo de cuenta bancaria inválido")]
        public string TipoCuentaBancaria { get; set; } = null!;

        public bool ContribuyenteEspecial { get; set; } = false;

        public bool ObligadoContabilidad { get; set; } = false;
    }
}