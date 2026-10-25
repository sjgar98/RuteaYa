using RuteaYa.MVC.Servicios;
using RuteaYa.MVC.Servicios.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Servicios del MVC (carpeta Servicios): wrappers HTTP que llaman a la API.
// Cada uno se registra como HttpClient tipado con TokenHandler para enviar el JWT del usuario.
var apiUrl = new Uri(builder.Configuration["ApiUrl"]!);
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenHandler>();

builder.Services.AddHttpClient<IViajeServicio, ViajeServicio>(c => c.BaseAddress = apiUrl)
    .AddHttpMessageHandler<TokenHandler>();

// TODO Autenticación: cookie del MVC. El login guarda en ella el JWT de la API como claim "token".
// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie(options => options.LoginPath = "/Cuenta/Login");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// TODO Autenticación: descomentar junto con AddAuthentication.
// app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
   .WithStaticAssets();

app.Run();
