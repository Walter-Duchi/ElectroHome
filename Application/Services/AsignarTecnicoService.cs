using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class AsignarTecnicoService
    {
        private readonly IAsignarTecnicoRepository _repo;

        public AsignarTecnicoService(IAsignarTecnicoRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> AsignarTecnico(int detalleCompraId)
        {
            var marcaId = await _repo.ObtenerMarcaPorDetalleCompra(detalleCompraId);

            var tecnicos = await _repo.ObtenerTecnicosCertificados(marcaId);

            if (!tecnicos.Any())
                throw new Exception($"No existe tecnicos certificados para la marca {marcaId}");

            var cargaTecnico = await _repo.ObtenerCargaTecnicos();

            var tecnicoAsignado = tecnicos
                .OrderBy(t =>
                {
                    var cargaTec = cargaTecnico.FirstOrDefault(ct => ct.TecnicoId == t.Id);

                    return (cargaTec.TecnicoId == 0) ? 0 : cargaTec.cantidad;
                })
                .First();


            return tecnicoAsignado.Id;
        }
    }

}
