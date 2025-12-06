using Application.Services;
namespace API.Endpoints
{
    public static class AsignarTecnico
    {
        public static void MapAsignarTecnico(this WebApplication app)
        {
            app.MapPost("/asignar-tecnico/{detalleCompraId}", async (int detalleCompraId, AsignarTecnicoService service) =>
            {
                try
                {
                    var tecnicoId = await service.AsignarTecnico(detalleCompraId);
                    return Results.Ok(new { TecnicoAsignado =  tecnicoId });

                }catch(Exception ex)
                {
                    return Results.BadRequest(new { Mensaje = ex.Message });
                }
            
            });
        }
    }
}
