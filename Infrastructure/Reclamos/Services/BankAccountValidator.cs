using System.Text.RegularExpressions;

namespace Infrastructure.Reclamos.Services
{
    public interface IBankAccountValidator
    {
        bool ValidateBankAccount(string accountNumber);
        bool ValidateAccountType(string accountType);
    }

    public class BankAccountValidator : IBankAccountValidator
    {
        public bool ValidateBankAccount(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                return false;

            // Eliminar espacios, guiones y otros caracteres
            accountNumber = accountNumber.Replace(" ", "").Replace("-", "").Replace(".", "");

            // Validar que solo contenga dígitos
            if (!accountNumber.All(char.IsDigit))
                return false;

            // Validar longitud (generalmente entre 10 y 20 dígitos para cuentas bancarias)
            if (accountNumber.Length < 10 || accountNumber.Length > 20)
                return false;

            return true;
        }

        public bool ValidateAccountType(string accountType)
        {
            if (string.IsNullOrWhiteSpace(accountType))
                return false;

            return accountType == "Ahorro" || accountType == "Corriente";
        }
    }
}