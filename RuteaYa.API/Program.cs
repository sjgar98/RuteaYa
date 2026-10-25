using Microsoft.EntityFrameworkCore;
using RuteaYa.API.Servicios;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.Entidades;
using RuteaYa.Repositorios;
using RuteaYa.Repositorios.Interfaces;

var builder = WebApplication.CreateBuilder(args);

using var db = new RuteaYaDbContext();
db.Database.Migrate();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "RuteaYa.API.xml"));
});

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IViajeRepositorio, ViajeRepositorio>();
builder.Services.AddScoped<IEnvioRepositorio, EnvioRepositorio>();
builder.Services.AddScoped<IAsignacionRepositorio, AsignacionRepositorio>();

builder.Services.AddScoped<IViajeServicio, ViajeServicio>();

builder.Services.AddDbContext<RuteaYaDbContext>();

// TODO Autenticación: validar el JWT que emite el login (requiere el paquete
// Microsoft.AspNetCore.Authentication.JwtBearer y la clave "Jwt:Key" en appsettings).
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = false,
//             ValidateAudience = false,
//             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
//         };
//     });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// TODO Autenticación: descomentar junto con AddAuthentication.
// app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
