using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AgroStock.Api.Data;
using AgroStock.Api.Exceptions;
using AgroStock.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Botón "Authorize" en Swagger para probar los endpoints protegidos con el token del login
    var esquema = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", esquema);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [esquema] = Array.Empty<string>() });
});

// Versión fija (no AutoDetect) para que "dotnet ef" funcione sin tener la base levantada
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MariaDbServerVersion(new Version(builder.Configuration["MariaDbVersion"] ?? "11.0"));
builder.Services.AddDbContext<AgroStockDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

var origenesFrontend = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendReact", policy =>
        policy.WithOrigins(origenesFrontend)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICultivoService, CultivoService>();
builder.Services.AddScoped<ICosechaService, CosechaService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IVentaService, VentaService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await DatosIniciales.InicializarAsync(app.Services, app.Configuration);
}

app.UseCors("FrontendReact");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
