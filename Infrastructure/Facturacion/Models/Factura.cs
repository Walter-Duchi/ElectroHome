using System.Xml.Serialization;

namespace Infrastructure.Facturacion.Models
{
    [XmlRoot("factura")]
    public class Factura
    {
        [XmlAttribute("id")]
        public string Id { get; set; } = "comprobante";

        [XmlAttribute("version")]
        public string Version { get; set; } = "1.0.0";

        public InfoTributaria InfoTributaria { get; set; } = new InfoTributaria();
        public InfoFactura InfoFactura { get; set; } = new InfoFactura();

        [XmlArray("detalles")]
        [XmlArrayItem("detalle")]
        public List<Detalle> Detalles { get; set; } = new List<Detalle>();

        public InfoAdicional InfoAdicional { get; set; } = new InfoAdicional();
    }

    public class InfoTributaria
    {
        public string ambiente { get; set; } = string.Empty;          // 1 = pruebas, 2 = producción
        public string tipoEmision { get; set; } = string.Empty;       // 1 = normal
        public string razonSocial { get; set; } = string.Empty;
        public string nombreComercial { get; set; } = string.Empty;
        public string ruc { get; set; } = string.Empty;
        public string claveAcceso { get; set; } = string.Empty;
        public string codDoc { get; set; } = string.Empty;            // 01 = factura
        public string estab { get; set; } = string.Empty;             // 3 dígitos
        public string ptoEmi { get; set; } = string.Empty;            // 3 dígitos
        public string secuencial { get; set; } = string.Empty;        // 9 dígitos
        public string dirMatriz { get; set; } = string.Empty;
    }

    public class InfoFactura
    {
        public string fechaEmision { get; set; } = string.Empty;      // dd/mm/aaaa
        public string dirEstablecimiento { get; set; } = string.Empty;
        public string? contribuyenteEspecial { get; set; } // opcional
        public string obligadoContabilidad { get; set; } = string.Empty;   // SI/NO
        public string tipoIdentificacionComprador { get; set; } = string.Empty; // 04=RUC, 05=Cédula, 07=Consumidor final
        public string razonSocialComprador { get; set; } = string.Empty;
        public string identificacionComprador { get; set; } = string.Empty;
        public string direccionComprador { get; set; } = string.Empty;
        public decimal totalSinImpuestos { get; set; }
        public decimal totalDescuento { get; set; }

        [XmlArray("totalConImpuestos")]
        [XmlArrayItem("totalImpuesto")]
        public List<TotalImpuesto> totalConImpuestos { get; set; } = new List<TotalImpuesto>();

        public decimal propina { get; set; }
        public decimal importeTotal { get; set; }
        public string moneda { get; set; } = "DOLAR";

        [XmlArray("pagos")]
        [XmlArrayItem("pago")]
        public List<Pago> pagos { get; set; } = new List<Pago>();
    }

    public class TotalImpuesto
    {
        public string codigo { get; set; } = string.Empty;            // 2 = IVA
        public string codigoPorcentaje { get; set; } = string.Empty;  // 2,0,6 según tarifa
        public decimal baseImponible { get; set; }
        public decimal valor { get; set; }
    }

    public class Detalle
    {
        public string codigoPrincipal { get; set; } = string.Empty;
        public string codigoAuxiliar { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal cantidad { get; set; }
        public decimal precioUnitario { get; set; }
        public decimal descuento { get; set; }
        public decimal precioTotalSinImpuesto { get; set; }

        [XmlArray("impuestos")]
        [XmlArrayItem("impuesto")]
        public List<DetalleImpuesto> impuestos { get; set; } = new List<DetalleImpuesto>();
    }

    public class DetalleImpuesto
    {
        public string codigo { get; set; } = string.Empty;            // 2 = IVA
        public string codigoPorcentaje { get; set; } = string.Empty;
        public decimal tarifa { get; set; }
        public decimal baseImponible { get; set; }
        public decimal valor { get; set; }
    }

    public class Pago
    {
        [XmlElement("formaPago")]
        public string formaPago { get; set; } = string.Empty;

        [XmlElement("total")]
        public decimal total { get; set; }

        [XmlElement("plazo")]
        public int? plazo { get; set; }

        [XmlElement("unidadTiempo")]
        public string? unidadTiempo { get; set; }

        // Estas propiedades controlan que no se serialicen si son nulas
        public bool ShouldSerializeplazo() => plazo.HasValue;
        public bool ShouldSerializeunidadTiempo() => !string.IsNullOrEmpty(unidadTiempo);
    }

    public class InfoAdicional
    {
        [XmlElement("campoAdicional")]
        public List<CampoAdicional> Campos { get; set; } = new List<CampoAdicional>();
    }

    public class CampoAdicional
    {
        [XmlAttribute("nombre")]
        public string Nombre { get; set; } = string.Empty;
        [XmlText]
        public string Valor { get; set; } = string.Empty;
    }
}