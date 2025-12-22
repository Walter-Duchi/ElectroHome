namespace Infrastructure.Services
{
    public interface IBankCardValidator
    {
        bool ValidateCardNumber(string cardNumber);
        string GetCardType(string cardNumber);
    }
}