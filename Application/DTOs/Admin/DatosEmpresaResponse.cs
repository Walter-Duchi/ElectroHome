namespace Application.DTOs.Admin
{
    public class DatosEmpresaResponse
    {
        public int Id { get; set; }
        public string RucEmpresa { get; set; } = string.Empty;
        public string NombreComercial { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string DireccionMatriz { get; set; } = string.Empty;
    }
}