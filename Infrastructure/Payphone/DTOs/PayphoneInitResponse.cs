namespace Infrastructure.Payphone.DTOs
{
    public class PayphoneInitResponse
    {
        public string ClientTransactionId { get; set; } = string.Empty;
        public int Amount { get; set; } // en centavos
        public int AmountWithoutTax { get; set; }
        public int AmountWithTax { get; set; }
        public int Tax { get; set; }
        public string Token { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
    }
}