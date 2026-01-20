namespace Application.DTOs.Reclamos.Tecnico
{
    public class FinalizarRevisionRequest
    {
        public int ReclamoProductoSnId { get; set; }
        public int TecnicoId { get; set; }
        public string Estado { get; set; } = null!; // "Aprobado" o "Rechazado"
        public string Explicacion { get; set; } = null!;
        public string? PdfBase64 { get; set; }
        public string? PdfFileName { get; set; }
    }
}