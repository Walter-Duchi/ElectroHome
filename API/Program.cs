using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//inicio
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ReclamosContext>(
    opts => opts.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
//fin

builder.Services.AddControllers();
builder.Services.AddOpenApi();
var app = builder.Build();

//inicio 
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/reclamos", async (ReclamosContext db) =>
    await db.Reclamos.ToListAsync()
);
//fin

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
