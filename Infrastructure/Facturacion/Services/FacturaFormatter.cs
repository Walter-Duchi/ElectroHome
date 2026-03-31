using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Infrastructure.Facturacion.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Facturacion.Services
{
    public static class FacturaFormatter
    {
        static FacturaFormatter()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static string GenerarHtmlDesdeXml(string xml)
        {
            try
            {
                var doc = XDocument.Parse(xml);
                var root = doc.Root;
                if (root == null) return "<p>No se pudo parsear la factura</p>";

                var infoTributaria = root.Element("infoTributaria");
                var infoFactura = root.Element("infoFactura");
                var detalles = root.Element("detalles")?.Elements("detalle").ToList() ?? new List<XElement>();

                var html = new StringBuilder();
                html.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>");
                html.AppendLine("<h2 style='text-align: center; color: #0056b3;'>FACTURA ELECTRÓNICA</h2>");
                html.AppendLine("<hr/>");

                // Emisor
                html.AppendLine("<div style='margin-bottom: 20px;'>");
                html.AppendLine($"<p><strong>Razón Social:</strong> {infoTributaria?.Element("razonSocial")?.Value}</p>");
                html.AppendLine($"<p><strong>RUC:</strong> {infoTributaria?.Element("ruc")?.Value}</p>");
                html.AppendLine($"<p><strong>Dirección Matriz:</strong> {infoTributaria?.Element("dirMatriz")?.Value}</p>");
                html.AppendLine("</div>");

                // Cliente
                html.AppendLine("<div style='margin-bottom: 20px;'>");
                html.AppendLine($"<p><strong>Cliente:</strong> {infoFactura?.Element("razonSocialComprador")?.Value}</p>");
                html.AppendLine($"<p><strong>Identificación:</strong> {infoFactura?.Element("identificacionComprador")?.Value}</p>");
                html.AppendLine($"<p><strong>Dirección:</strong> {infoFactura?.Element("direccionComprador")?.Value}</p>");
                html.AppendLine("</div>");

                // Datos factura
                html.AppendLine("<div style='margin-bottom: 20px;'>");
                html.AppendLine($"<p><strong>Fecha Emisión:</strong> {infoFactura?.Element("fechaEmision")?.Value}</p>");
                html.AppendLine($"<p><strong>Clave de Acceso:</strong> {infoTributaria?.Element("claveAcceso")?.Value}</p>");
                html.AppendLine($"<p><strong>Número Autorización:</strong> {infoTributaria?.Element("secuencial")?.Value}</p>");
                html.AppendLine("</div>");

                // Detalles
                html.AppendLine("<table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>");
                html.AppendLine("<thead><tr style='background-color: #f2f2f2;'>");
                html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Descripción</th>");
                html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: right;'>Cantidad</th>");
                html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: right;'>Precio Unitario</th>");
                html.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: right;'>Subtotal</th>");
                html.AppendLine("</thead><tbody>");

                foreach (var det in detalles)
                {
                    string desc = det.Element("descripcion")?.Value ?? "";
                    string cant = det.Element("cantidad")?.Value ?? "0";
                    string pUnit = det.Element("precioUnitario")?.Value ?? "0";
                    string pTotal = det.Element("precioTotalSinImpuesto")?.Value ?? "0";
                    html.AppendLine("税");
                    html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{desc}</td>");
                    html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{cant}</td>");
                    html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>${decimal.Parse(pUnit):F2}</td>");
                    html.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>${decimal.Parse(pTotal):F2}</td>");
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</tbody></table>");

                // Totales
                var totalSinImp = infoFactura?.Element("totalSinImpuestos")?.Value ?? "0";
                var totalIva = infoFactura?.Element("totalConImpuestos")?.Elements("totalImpuesto").FirstOrDefault()?.Element("valor")?.Value ?? "0";
                var total = infoFactura?.Element("importeTotal")?.Value ?? "0";
                html.AppendLine("<div style='text-align: right; margin-top: 20px;'>");
                html.AppendLine($"<p><strong>Subtotal sin impuestos:</strong> ${decimal.Parse(totalSinImp):F2}</p>");
                html.AppendLine($"<p><strong>IVA (15%):</strong> ${decimal.Parse(totalIva):F2}</p>");
                html.AppendLine($"<p><strong>Total a pagar:</strong> ${decimal.Parse(total):F2}</p>");
                html.AppendLine("</div>");

                html.AppendLine("<hr/>");
                html.AppendLine("<p style='text-align: center; font-size: 12px; color: #777;'>Documento autorizado por el SRI</p>");
                html.AppendLine("</div>");
                return html.ToString();
            }
            catch (Exception ex)
            {
                return $"<p>Error al generar HTML: {ex.Message}</p>";
            }
        }

        public static byte[] GenerarPdfDesdeXml(string xml)
        {
            // Deserializar XML a objeto Factura
            Factura factura;
            using (var reader = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(Factura));
                factura = (Factura)serializer.Deserialize(reader);
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Content().Column(column =>
                    {
                        column.Item().Text("FACTURA ELECTRÓNICA")
                            .FontSize(18).Bold().AlignCenter();

                        column.Item().PaddingBottom(10).LineHorizontal(0.5f);

                        // Info Tributaria (Emisor)
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("EMISOR").Bold().FontSize(12);
                                col.Item().Text($"Razón Social: {factura.InfoTributaria.razonSocial}");
                                col.Item().Text($"RUC: {factura.InfoTributaria.ruc}");
                                col.Item().Text($"Dirección Matriz: {factura.InfoTributaria.dirMatriz}");
                            });
                        });

                        column.Item().PaddingBottom(10).LineHorizontal(0.5f);

                        // Info Factura (Cliente y datos de factura)
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("CLIENTE").Bold().FontSize(12);
                                col.Item().Text($"Razón Social: {factura.InfoFactura.razonSocialComprador}");
                                col.Item().Text($"Identificación: {factura.InfoFactura.identificacionComprador}");
                                col.Item().Text($"Dirección: {factura.InfoFactura.direccionComprador}");
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("DETALLES DE FACTURA").Bold().FontSize(12);
                                col.Item().Text($"Fecha Emisión: {factura.InfoFactura.fechaEmision}");
                                col.Item().Text($"Clave de Acceso: {factura.InfoTributaria.claveAcceso}");
                                col.Item().Text($"Número Autorización: {factura.InfoTributaria.secuencial}");
                            });
                        });

                        column.Item().PaddingBottom(10).LineHorizontal(0.5f);

                        // Tabla de productos
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Descripción").Bold();
                                header.Cell().Text("Cantidad").Bold().AlignRight();
                                header.Cell().Text("Precio Unitario").Bold().AlignRight();
                                header.Cell().Text("Subtotal").Bold().AlignRight();
                            });

                            foreach (var detalle in factura.Detalles)
                            {
                                table.Cell().Text(detalle.descripcion);
                                table.Cell().Text(detalle.cantidad.ToString()).AlignRight();
                                table.Cell().Text($"${detalle.precioUnitario:F2}").AlignRight();
                                table.Cell().Text($"${detalle.precioTotalSinImpuesto:F2}").AlignRight();
                            }
                        });

                        column.Item().PaddingBottom(10).LineHorizontal(0.5f);

                        // Totales
                        var totalSinImp = factura.InfoFactura.totalSinImpuestos;
                        var totalIva = factura.InfoFactura.totalConImpuestos.FirstOrDefault()?.valor ?? 0;
                        var total = factura.InfoFactura.importeTotal;

                        column.Item().AlignRight().Column(col =>
                        {
                            col.Item().Text($"Subtotal sin impuestos: ${totalSinImp:F2}");
                            col.Item().Text($"IVA (15%): ${totalIva:F2}");
                            col.Item().Text($"Total a pagar: ${total:F2}").Bold();
                        });

                        column.Item().PaddingBottom(10).LineHorizontal(0.5f);
                        column.Item().Text("Documento autorizado por el SRI")
                            .FontSize(9).Italic().AlignCenter();
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }
    }
}