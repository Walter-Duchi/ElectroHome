using System;

namespace Infrastructure.Payphone.DTOs
{
    public class PayphoneConfirmResponse
    {
        public long TransactionId { get; set; }
        public string ClientTransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Approved, Canceled, etc.
        public int StatusCode { get; set; }
        public decimal Amount { get; set; }
        public string AuthorizationCode { get; set; } = string.Empty;
        public string? Message { get; set; }
        public int? VentaId { get; set; }
        public string? PdfUrl { get; set; }
    }
}