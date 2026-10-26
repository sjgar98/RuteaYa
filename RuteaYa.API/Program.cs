using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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

    // Botón "Authorize" de Swagger: pegar el token que devuelve POST api/usuarios/login.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token devuelto por POST api/usuarios/login (sin el prefijo \"Bearer\")."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IViajeRepositorio, ViajeRepositorio>();
builder.Services.AddScoped<IEnvioRepositorio, EnvioRepositorio>();
builder.Services.AddScoped<IAsignacionRepositorio, AsignacionRepositorio>();

builder.Services.AddScoped<IViajeServicio, ViajeServicio>();
builder.Services.AddScoped<IUsuarioServicio, UsuarioServicio>();
builder.Services.AddScoped<ITokenServicio, TokenServicio>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<IRutaServicio, RutaServicioMock>();

builder.Services.AddDbContext<RuteaYaDbContext>();

// Valida el JWT que emite TokenServicio (misma clave "Jwt:Key" de appsettings).
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Todo endpoint exige token salvo los marcados con [AllowAnonymous] (registro y login),
// aunque alguien se olvide del [Authorize] en un controller nuevo.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // En desarrollo el MVC llama por http (ApiUrl de appsettings.Development.json). Si la API lo
    // redirigiera a https, HttpClient seguiría la redirección sin el header Authorization y daría 401.
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
