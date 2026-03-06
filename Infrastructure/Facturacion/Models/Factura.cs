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

        [XmlElement("infoTributaria")]
        public InfoTributaria InfoTributaria { get; set; } = new InfoTributaria();

        [XmlElement("infoFactura")]
        public InfoFactura InfoFactura { get; set; } = new InfoFactura();

        [XmlArray("detalles")]
        [XmlArrayItem("detalle")]
        public List<Detalle> Detalles { get; set; } = new List<Detalle>();

        [XmlElement("infoAdicional")]
        public InfoAdicional InfoAdicional { get; set; } = new InfoAdicional();
    }

    public class InfoTributaria
    {
        public string ambiente { get; set; } = string.Empty;
        public string tipoEmision { get; set; } = string.Empty;
        public string razonSocial { get; set; } = string.Empty;
        public string nombreComercial { get; set; } = string.Empty;
        public string ruc { get; set; } = string.Empty;
        public string claveAcceso { get; set; } = string.Empty;
        public string codDoc { get; set; } = string.Empty;
        public string estab { get; set; } = string.Empty;
        public string ptoEmi { get; set; } = string.Empty;
        public string secuencial { get; set; } = string.Empty;
        public string dirMatriz { get; set; } = string.Empty;
    }

    public class InfoFactura
    {
        public string fechaEmision { get; set; } = string.Empty;
        public string dirEstablecimiento { get; set; } = string.Empty;
        public string? contribuyenteEspecial { get; set; }
        public string obligadoContabilidad { get; set; } = string.Empty;
        public string tipoIdentificacionComprador { get; set; } = string.Empty;
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
        public string codigo { get; set; } = string.Empty;
        public string codigoPorcentaje { get; set; } = string.Empty;
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
        public string codigo { get; set; } = string.Empty;
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

        [XmlIgnore] // No se serializa si es nulo
        public int? plazo { get; set; }

        [XmlIgnore]
        public string? unidadTiempo { get; set; }

        // Propiedades auxiliares para serializar solo si tienen valor
        [XmlElement("plazo")]
        public decimal PlazoSerializado
        {
            get => plazo ?? 0;
            set => plazo = (int)value;
        }

        [XmlElement("unidadTiempo")]
        public string UnidadTiempoSerializada
        {
            get => unidadTiempo ?? string.Empty;
            set => unidadTiempo = value;
        }

        public bool ShouldSerializePlazoSerializado() => plazo.HasValue;
        public bool ShouldSerializeUnidadTiempoSerializada() => !string.IsNullOrEmpty(unidadTiempo);
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