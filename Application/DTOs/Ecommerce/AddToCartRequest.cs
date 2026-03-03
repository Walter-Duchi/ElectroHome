namespace Application.DTOs.Ecommerce
{
    public class AddToCartRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
}