namespace Application.DTOs.Reclamos.Tecnico
{
    public class IniciarRevisionRequest
    {
        public int ReclamoProductoSnId { get; set; }
        public int TecnicoId { get; set; }
    }
}