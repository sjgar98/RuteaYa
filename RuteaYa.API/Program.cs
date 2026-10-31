using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.Extensions.Options;
using RuteaYa.API.Servicios;
using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.Entidades;
using RuteaYa.Repositorios;
using RuteaYa.Repositorios.Interfaces;
using RuteaYa.API.Servicios.Rutas;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

using var db = new RuteaYaDbContext();
db.Database.Migrate();

//Esto lo agrege xq no queria tocar cosas de las migraciones en la base de datos)? le tengo respeto 
//await InicializadorDatos.InicializarAsync(db);

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
builder.Services.AddMemoryCache();
builder.Services.Configure<RutasOpciones>(builder.Configuration.GetSection("Rutas"));
builder.Services.AddScoped<RutaServicioMock>();
builder.Services.AddHttpClient<RutaServicioGoogle>(cliente => cliente.Timeout = TimeSpan.FromSeconds(10));
builder.Services.AddScoped<IRutaServicio>(sp =>
{
    var opciones = sp.GetRequiredService<IOptions<RutasOpciones>>().Value;
    var mock = sp.GetRequiredService<RutaServicioMock>();

    if (!opciones.UsarGoogle())
    {
        return mock;
    }

    var google = sp.GetRequiredService<RutaServicioGoogle>();
    if (!opciones.RespaldoAlMock)
    {
        return google;
    }

    return new RutaServicioConRespaldo(
        google, mock, sp.GetRequiredService<IMemoryCache>(), sp.GetRequiredService<ILogger<RutaServicioConRespaldo>>());
});
builder.Services.AddScoped<IHistorialServicio, HistorialServicio>();
builder.Services.AddScoped<IEnvioServicio, EnvioServicio>();
builder.Services.AddScoped<ITransporteServicio, TransporteServicio>();
builder.Services.AddScoped<IOportunidadesServicio, OportunidadesServicio>();
builder.Services.AddScoped<IOptimizadorServicio, OptimizadorServicio>();

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

//app.Logger.LogInformation("Servicio de rutas: {Proveedor}",
//    app.Services.GetRequiredService<IOptions<RutasOpciones>>().Value.UsarGoogle() ? "Google" : "Mock");
var opcionesRutas = app.Services.GetRequiredService<IOptions<RutasOpciones>>().Value;
app.Logger.LogInformation("Servicio de rutas: {Proveedor}",
    !opcionesRutas.UsarGoogle() ? "Mock"
    : opcionesRutas.RespaldoAlMock ? "Google (con respaldo al mock)"
    : "Google (sin respaldo)");

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
