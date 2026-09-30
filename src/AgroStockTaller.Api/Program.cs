using Microsoft.EntityFrameworkCore;
using AgroStockTaller.Api.Data;
using AgroStockTaller.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Base de datos en memoria: suficiente para correr y demostrar el taller.
// (El proyecto completo de AgroStock usa MariaDB; eso queda fuera del alcance
// de esta parte del taller, ver README).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("AgroStockTallerDb"));

builder.Services.AddScoped<ICultivoService, CultivoService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
