using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using RuteaYa.MVC.Filtros;
using RuteaYa.MVC.Servicios;
using RuteaYa.MVC.Servicios.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.Filters.Add<SesionVencidaFilter>());

// Servicios del MVC (carpeta Servicios): wrappers HTTP que llaman a la API.
// Cada uno se registra como HttpClient tipado con TokenHandler para enviar el JWT del usuario.
var apiUrl = new Uri(builder.Configuration["ApiUrl"]!);
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenHandler>();

builder.Services.AddHttpClient<IViajeServicio, ViajeServicio>(c => c.BaseAddress = apiUrl)
    .AddHttpMessageHandler<TokenHandler>();

// Login y registro no llevan token: la API los permite sin autenticación.
builder.Services.AddHttpClient<ICuentaServicio, CuentaServicio>(c => c.BaseAddress = apiUrl);

// Cookie del MVC. El login guarda en ella el JWT de la API como claim "token".
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.LogoutPath = "/Cuenta/Logout";
        // Sin renovación: la cookie no puede durar más que el token que guarda.
        options.SlidingExpiration = false;
    });

// Toda pantalla exige sesión salvo las marcadas con [AllowAnonymous] (Login y Registro).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// CSS y JS públicos: la pantalla de login los necesita antes de iniciar sesión.
app.MapStaticAssets().AllowAnonymous();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
   .WithStaticAssets();

app.Run();
