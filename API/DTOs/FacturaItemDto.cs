namespace API.DTOs
{
    public record FacturaItemDto
    (
        string Marca,
        string Modelo,
        string NumSerie,
        DateTime VentaUsuarioFinal,
        int DiasDeGarantia,
        string EstadoGarantia
    );
}
