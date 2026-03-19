using System.Threading.Tasks;
using Infrastructure.Payphone.DTOs;

namespace Infrastructure.Payphone.Services
{
    public interface IPayphoneService
    {
        /// <summary>
        /// Inicializa una transacción pendiente en la base de datos y devuelve los datos necesarios para la cajita.
        /// </summary>
        Task<PayphoneInitResponse> InitializeTransactionAsync(int usuarioId);

        /// <summary>
        /// Confirma la transacción con Payphone y, si es exitosa, crea la venta y factura.
        /// </summary>
        Task<PayphoneConfirmResponse> ConfirmTransactionAsync(PayphoneConfirmRequest request, int usuarioId);
    }
}