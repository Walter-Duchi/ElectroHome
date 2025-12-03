using Application.Interfaces;
using Application.Models;
using Infrastructure.Data;
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
            return 0;
        }

        public async Task<List<TecnicoInfo>> ObtenerTecnicosCertificados(int marcaId)
        {
            List<TecnicoInfo> listaTecnico = new List<TecnicoInfo>();
            return listaTecnico;
        }

        public async Task<List<(int TecnicoId, int cantidad)>> ObtenerCargaTecnicos()
        {
            var resultado = new List<(int TecnicoId, int cantidad)>
            {
                (1, 5)
            };

            return resultado;
        }

    }
}
