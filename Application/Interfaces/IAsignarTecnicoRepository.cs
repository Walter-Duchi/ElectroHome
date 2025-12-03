using Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IAsignarTecnicoRepository
    {
        Task<int> ObtenerMarcaPorDetalleCompra(int detalleCompraId);
        Task<List<TecnicoInfo>> ObtenerTecnicosCertificados(int marcaId);
        Task<List<(int TecnicoId, int cantidad)>> ObtenerCargaTecnicos();
    }
}
