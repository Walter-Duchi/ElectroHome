namespace Infrastructure.Payphone.DTOs
{
    public class PayphoneConfirmRequest
    {
        public long Id { get; set; }          // id devuelto por Payphone en la URL
        public string ClientTransactionId { get; set; } = string.Empty;
    }
}