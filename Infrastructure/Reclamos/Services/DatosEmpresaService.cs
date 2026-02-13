using Application.DTOs.Admin;
using Infrastructure.Data;
using Infrastructure.Models;
using Infrastructure.Reclamos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Reclamos.Services
{
    public class DatosEmpresaService : IDatosEmpresaService
    {
        private readonly ReclamosContext _context;

        public DatosEmpresaService(ReclamosContext context)
        {
            _context = context;
        }

        public async Task<DatosEmpresaResponse?> ObtenerDatosEmpresaAsync()
        {
            var empresa = await _context.DatosEmpresas.FirstOrDefaultAsync();
            if (empresa == null)
                return null;

            return new DatosEmpresaResponse
            {
                Id = empresa.Id,
                RucEmpresa = empresa.RucEmpresa,
                NombreComercial = empresa.NombreComercial,
                RazonSocial = empresa.RazonSocial,
                DireccionMatriz = empresa.DireccionMatriz
            };
        }

        public async Task<DatosEmpresaResponse> ActualizarDatosEmpresaAsync(UpdateDatosEmpresaRequest request)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(request.RucEmpresa))
                throw new ArgumentException("El RUC de la empresa es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.NombreComercial))
                throw new ArgumentException("El nombre comercial es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.RazonSocial))
                throw new ArgumentException("La razón social es obligatoria.");
            if (string.IsNullOrWhiteSpace(request.DireccionMatriz))
                throw new ArgumentException("La dirección matriz es obligatoria.");

            // Buscar si ya existe un registro
            var empresa = await _context.DatosEmpresas.FirstOrDefaultAsync();

            if (empresa == null)
            {
                // Crear nuevo
                empresa = new DatosEmpresa
                {
                    RucEmpresa = request.RucEmpresa,
                    NombreComercial = request.NombreComercial,
                    RazonSocial = request.RazonSocial,
                    DireccionMatriz = request.DireccionMatriz
                };
                _context.DatosEmpresas.Add(empresa);
            }
            else
            {
                // Actualizar existente
                empresa.RucEmpresa = request.RucEmpresa;
                empresa.NombreComercial = request.NombreComercial;
                empresa.RazonSocial = request.RazonSocial;
                empresa.DireccionMatriz = request.DireccionMatriz;
            }

            await _context.SaveChangesAsync();

            return new DatosEmpresaResponse
            {
                Id = empresa.Id,
                RucEmpresa = empresa.RucEmpresa,
                NombreComercial = empresa.NombreComercial,
                RazonSocial = empresa.RazonSocial,
                DireccionMatriz = empresa.DireccionMatriz
            };
        }
    }
}