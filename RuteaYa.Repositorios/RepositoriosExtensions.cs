using Microsoft.Extensions.DependencyInjection;
using RuteaYa.Repositorios.Implementaciones;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.Repositorios
{
    public static class RepositoriosExtensions
    {
        public static IServiceCollection AddRepositorios(this IServiceCollection services)
        {
            services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
            services.AddScoped<IViajeRepositorio, ViajeRepositorio>();
            services.AddScoped<IParadaRepositorio, ParadaRepositorio>();
            services.AddScoped<IEnvioRepositorio, EnvioRepositorio>();
            services.AddScoped<IAsignacionRepositorio, AsignacionRepositorio>();
            services.AddScoped<IEnvioEstadoHistorialRepositorio, EnvioEstadoHistorialRepositorio>();
            return services;
        }
    }
}
