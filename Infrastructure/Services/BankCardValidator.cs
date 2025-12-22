using System.Linq;
using System.Text.RegularExpressions;

namespace Infrastructure.Services
{
    public class BankCardValidator : IBankCardValidator
    {
        public bool ValidateCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            if (!cardNumber.All(char.IsDigit))
                return false;

            if (cardNumber.Length < 13 || cardNumber.Length > 19)
                return false;

            return IsValidLuhn(cardNumber);
        }

        public string GetCardType(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return "Unknown";

            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            if (Regex.IsMatch(cardNumber, @"^4[0-9]{12}(?:[0-9]{3})?$"))
                return "Visa";

            if (Regex.IsMatch(cardNumber, @"^5[1-5][0-9]{14}$"))
                return "Mastercard";

            if (Regex.IsMatch(cardNumber, @"^3[47][0-9]{13}$"))
                return "American Express";

            if (Regex.IsMatch(cardNumber, @"^3(?:0[0-5]|[68][0-9])[0-9]{11}$"))
                return "Diners Club";

            if (Regex.IsMatch(cardNumber, @"^6(?:011|5[0-9]{2})[0-9]{12}$"))
                return "Discover";

            if (Regex.IsMatch(cardNumber, @"^(?:2131|1800|35\d{3})\d{11}$"))
                return "JCB";

            return "Unknown";
        }

        private bool IsValidLuhn(string cardNumber)
        {
            int sum = 0;
            bool alternate = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = cardNumber[i] - '0';

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }
    }
}