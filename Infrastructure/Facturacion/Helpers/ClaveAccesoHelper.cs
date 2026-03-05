using System;

namespace Infrastructure.Facturacion.Helpers
{
    public static class ClaveAccesoHelper
    {
        public static string GenerarClaveAcceso(DateTime fechaEmision, string ruc, string estab, string ptoEmi, string secuencial)
        {
            // Formato: ddmmaaaa + 01 + ruc + 1 + estab(3) + ptoEmi(3) + secuencial(9) + codigoNumerico(8) + 1
            string fecha = fechaEmision.ToString("ddMMyyyy");
            string tipoComp = "01"; // factura
            string ambiente = "1";   // pruebas
            string serie = estab.PadLeft(3, '0') + ptoEmi.PadLeft(3, '0'); // 6 dígitos
            string sec = secuencial.PadLeft(9, '0');
            Random rnd = new Random();
            string codigoNumerico = rnd.Next(10000000, 99999999).ToString();
            string tipoEmision = "1";

            string baseClave = fecha + tipoComp + ruc + ambiente + serie + sec + codigoNumerico + tipoEmision;
            // baseClave debe tener 48 dígitos
            string digito = CalcularDigitoModulo11(baseClave);
            return baseClave + digito;
        }

        private static string CalcularDigitoModulo11(string baseClave)
        {
            int[] factores = { 2, 3, 4, 5, 6, 7 }; // se repiten cíclicamente
            int suma = 0;
            for (int i = baseClave.Length - 1, j = 0; i >= 0; i--, j++)
            {
                int digito = int.Parse(baseClave[i].ToString());
                int factor = factores[j % factores.Length];
                suma += digito * factor;
            }
            int residuo = suma % 11;
            int digitoVerificador = 11 - residuo;
            if (digitoVerificador == 11) digitoVerificador = 0;
            if (digitoVerificador == 10) digitoVerificador = 1;
            return digitoVerificador.ToString();
        }
    }
}