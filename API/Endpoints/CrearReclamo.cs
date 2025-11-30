using Infrastructure.Data;

namespace API.Endpoints
{
    public static class CrearReclamo
    {
        public static void MapCrearReclamo(this WebApplication app) {
            app.MapPost("/reclamo/{ProductoUnico}", async (string ProductoUnico, ReclamosContext db) =>
            {


            })
            .WithName("PostCrearReclamoById");
        }
    }
}
