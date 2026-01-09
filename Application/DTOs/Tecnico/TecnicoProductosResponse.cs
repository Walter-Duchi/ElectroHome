namespace Application.DTOs.Tecnico
{
    public class TecnicoProductosResponse
    {
        public int Id { get; set; }
        public string NumeroSerie { get; set; } = null!;
        public string Marca { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public string Especificacion { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public DateTime FechaReclamoClienteFinal { get; set; }
        public string CodigoReclamo { get; set; } = null!;
        public string FormaCompensacion { get; set; } = null!;
        public decimal Precio { get; set; }
        public string ClienteNombre { get; set; } = null!;
        public string ClienteRuc { get; set; } = null!;
        public DateTime FechaVentaClienteFinal { get; set; }
        public int DiasGarantia { get; set; }
        public bool GarantiaValida { get; set; }
    }
}