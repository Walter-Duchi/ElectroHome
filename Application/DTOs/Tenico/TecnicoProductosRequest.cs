namespace Application.DTOs.Tecnico
{
    public class TecnicoProductosRequest
    {
        public int TecnicoId { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Puede ser "Pendiente", "En Revision"
    }
}