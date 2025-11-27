using Microsoft.EntityFrameworkCore;

namespace API.Endpoints
{
    public class CrearReclamo
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/factura/{codigoFactura}", () => "Hello World");

            app.Run();
        }
    }
}
