namespace Application.DTOs.Reclamos.Reclamo
{
    public class CrearReclamoResponse
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        public int? ReclamoId { get; set; }
        public string? CodigoReclamo { get; set; }
        public string? PdfBase64 { get; set; }
        public string? PdfFileName { get; set; }
    }
}