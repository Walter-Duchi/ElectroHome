using System;
using System.Collections.Generic;

namespace Application.DTOs.Cliente
{
    public class ClienteDashboardResponse
    {
        public List<ClienteReclamoDTO> Reclamos { get; set; } = new();
        public EstadisticasClienteDTO Estadisticas { get; set; } = new();
    }

    public class ClienteReclamoDTO
    {
        public int ReclamoId { get; set; }
        public string CodigoReclamo { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public List<ClienteProductoDTO> Productos { get; set; } = new();
    }

    public class ClienteProductoDTO
    {
        public int ReclamoProductoId { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public string TipoReclamo { get; set; } = string.Empty; // Forma de Compensación
        public string Estado { get; set; } = string.Empty;
        public DateTime? FechaVentaClienteFinal { get; set; }
        public DateTime? FechaReclamoClienteFinal { get; set; }

        // Información del técnico
        public int? TecnicoId { get; set; }
        public string? TecnicoNombre { get; set; }
        public DateTime? FechaRevisionTecnico { get; set; }
        public string? ExplicacionRespuestaTecnico { get; set; }
        public string? PdfRevisionTecnico { get; set; }

        // Información de compensación
        public CompensacionDTO? Compensacion { get; set; }

        // Prioridad para ordenamiento
        public int Prioridad { get; set; }
    }

    public class CompensacionDTO
    {
        public string Tipo { get; set; } = string.Empty; // "Reembolso" o "Reemplazo"

        // Para reembolso
        public string? NumeroComprobanteReembolso { get; set; }
        public DateTime? FechaReembolso { get; set; }
        public string? NumCuentaBancariaReembolso { get; set; }

        // Para reemplazo
        public string? NumeroSerieReemplazo { get; set; }
        public string? PdfComprobanteEntrega { get; set; }
        public string? PersonalEntregaNombre { get; set; }
    }

    public class EstadisticasClienteDTO
    {
        public int TotalReclamos { get; set; }
        public int ProductosPendientes { get; set; }
        public int ProductosEnRevision { get; set; }
        public int ProductosAprobados { get; set; }
        public int ProductosRechazados { get; set; }
        public int ProductosCompensados { get; set; }
        public int ReembolsosTotales { get; set; }
        public int ReemplazosTotales { get; set; }
    }

    public class ClienteDashboardRequest
    {
        public string? CodigoReclamo { get; set; }
        public string? NumeroSerie { get; set; }
        public string? TipoReclamo { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool? SoloPendientes { get; set; }
        public bool? SoloAprobados { get; set; }
        public bool? SoloCompensados { get; set; }
        public bool? SoloReembolsos { get; set; }
        public bool? SoloReemplazos { get; set; }
    }
}