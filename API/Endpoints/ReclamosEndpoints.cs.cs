using Domain.Data;

namespace API.Endpoints
{
    public static class ReclamosEndpoints
    {
        public static void MapReclamosRouter(this WebApplication app)
        {
            var api = app.MapGroup("/api").WithTags("Reclamos");

            api.MapGet("/factura/{codigoFactura}", async (string codigoFactura, ReclamosContext db) => {
                
            });
        }
    }
}
