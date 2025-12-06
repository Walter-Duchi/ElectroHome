using Application.Interfaces;
using Application.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class AsignarTecnicoRepository: IAsignarTecnicoRepository
    {
        private readonly ReclamosContext _context;

        public AsignarTecnicoRepository(ReclamosContext context)
        {
            _context = context;
        }
        
        public async Task<int> ObtenerMarcaPorDetalleCompra(int detalleCompraId)
        {
            var marcaId = await _context.DetallesCompras
                .Where(dc => dc.Id == detalleCompraId)
                .Select(dc => dc.FkNumeroSerieNavigation.FkProductoNavigation.FkMarca)
                .FirstOrDefaultAsync();

            return marcaId;

        }

        public async Task<List<TecnicoInfo>> ObtenerTecnicosCertificados(int marcaId)
        {
            return await _context.UsuariosCertificacionMarcas
                .Where(ucm => ucm.FkMarca == marcaId)
                .Select(ucm => new TecnicoInfo {
                    Id = ucm.FkTecnico,
                    NombreCompleto = ucm.FkTecnicoNavigation.Nombres + " " + ucm.FkTecnicoNavigation.Apellidos
                }).ToListAsync();
        }

        public async Task<List<(int TecnicoId, int cantidad)>> ObtenerCargaTecnicos()
        {
            var cargas = await _context.Reclamos
                .Where(r => r.FkTecnicoAsignado != null)
                .GroupBy(r => r.FkTecnicoAsignado)
                .Select(g => new
                {
                    TecnicoId = g.Key!.Value,
                    cantidad = g.Count()
                }).ToListAsync();

            return cargas
                .Select(r => (r.TecnicoId, r.cantidad))
                .ToList();
        }

    }
}
