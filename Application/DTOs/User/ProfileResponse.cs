namespace Application.DTOs.User
{
    public class ProfileResponse
    {
        public int Id { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string RazonSocial { get; set; }
        public string TipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public string Ruc { get; set; }
        public string Correo { get; set; }
        public string Celular { get; set; }
        public string Convencional { get; set; }
        public string Pais { get; set; }
        public string DivisionAdministrativa { get; set; }
        public string Ciudad { get; set; }
        public string CodigoPostal { get; set; }
        public string Direccion { get; set; }
        public string Rol { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string NumCuentaBancaria { get; set; }
        public string TipoCuentaBancaria { get; set; }
        public bool ContribuyenteEspecial { get; set; }
        public bool ObligadoContabilidad { get; set; }
        public bool Activo { get; set; }
    }
}