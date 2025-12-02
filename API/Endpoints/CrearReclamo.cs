using Infrastructure.Data;
using Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Models;
using Domain.Services;

namespace API.Endpoints
{
    public static class CrearReclamo
    {
        public static void MapCrearReclamo(this WebApplication app) {
            app.MapPost("/reclamo/crear", async (CrearReclamoDto dto, ReclamosContext db) =>
            {
                var serie = await db.NumeroSerieProductos
                    .FirstOrDefaultAsync(x => x.NumeroSerie == dto.NumeroSerie);

                if (serie is null)
                {
                    return Results.NotFound($"No existe un producto con el numero de serie {dto.NumeroSerie}");
                }

                var detalle = await db.DetallesCompras
                    .FirstOrDefaultAsync(x => x.FkNumeroSerie == dto.NumeroSerie);

                if (detalle is null)
                {
                    return Results.NotFound($"El producto {dto.NumeroSerie} existe pero no ha sido vendido aún");
                }

                var reclamoExistente = await db.Reclamos
                    .AnyAsync(r => r.FkDetalleCompra == detalle.Id);

                if (reclamoExistente)
                {
                    return Results.BadRequest($"El producto {dto.NumeroSerie} ya tiene reclamos registrados");
                }

                var compra = await db.Compras.FirstOrDefaultAsync(c => c.Id == detalle.FkCompra);

                var fechaVentaFinal = dto.FechaVentaClienteFinal ?? compra.FechaCompra;

                var Cliente = await db.Usuarios.FirstOrDefaultAsync(c => c.Id == compra.FkCliente);

                ReclamoCodeGenerator reclamo = new ReclamoCodeGenerator();

                var nuevoReclamo = new Reclamo
                {
                    CodigoReclamo = reclamo.GenerarCodigo(),
                    FkDetalleCompra = detalle.Id,
                    FkClienteFinal = Cliente.Id,
                    FechaVentaClienteFinal = (DateTime)fechaVentaFinal,
                    FechaReclamoClienteFinal = DateTime.Now,
                    Estado =  "Pendiente"
                };

                db.Reclamos.Add(nuevoReclamo);
                await db.SaveChangesAsync();

                return Results.Created($"reclamos/{nuevoReclamo.Id}", nuevoReclamo);
            
                
            })
            .WithName("PostCrearReclamo");
        }
    }
}
