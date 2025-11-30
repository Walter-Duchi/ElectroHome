using Application.DTOs;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints
{
    public static class VerFactura
    {
        public static void MapVerFactura(this WebApplication app)
        {
            app.MapGet("/factura/{codigoFactura}", async (string codigoFactura, ReclamosContext db) =>
            {
                var productos = await db.DetallesCompras
                    .Where(d => d.FkCompraNavigation.CodigoFactura == codigoFactura)
                    .Select(d => new Factura
                    {
                        Marca = d.FkNumeroSerieNavigation.FkProductoNavigation.FkMarcaNavigation.Nombre,
                        Modelo = d.FkNumeroSerieNavigation.FkProductoNavigation.Modelo,
                        NumSerie = d.FkNumeroSerie,
                        VentaUsuario = (DateTime)d.FkCompraNavigation.FechaCompra,
                        TiempoGarantia = d.FkNumeroSerieNavigation.FkProductoNavigation.DiasGarantia
                    })
                    .ToListAsync();

                if (!productos.Any())
                    return Results.NotFound(new { mensaje = $"No se encontraron productos para la factura '{codigoFactura}'." });

                return Results.Ok(productos);
            })
            .WithName("GetFacturaByCodigo")
            .Produces<List<Factura>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);   
        }
    }
}
