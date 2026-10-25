using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.API.Servicios
{
    public class ViajeServicio : IViajeServicio
    {
        private readonly IViajeRepositorio _repositorio;

        public ViajeServicio(IViajeRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public Task<List<ViajeDto>> ListarPorUsuarioAsync(int usuarioId, bool? activo)
        {
            // TODO Datos de prueba: reemplazar por la consulta al repositorio cuando haya datos reales
            // (y marcar el método como async).
            // var viajes = await _repositorio.ListarPorUsuarioAsync(usuarioId, activo);
            // return viajes.Select(ADto).ToList();

            var viajes = ViajesDePrueba()
                .Where(v => !activo.HasValue || v.Activo == activo.Value)
                .ToList();

            return Task.FromResult(viajes);
        }

        private static ViajeDto ADto(Viaje viaje)
        {
            return new ViajeDto
            {
                Id = viaje.Id,
                OrigenDir = viaje.OrigenDir,
                DestinoDir = viaje.DestinoDir,
                Dias = viaje.Dias.Select(d => d.DiaSemana).OrderBy(d => d).ToList(),
                HoraSalida = viaje.HoraSalida,
                HoraLlegada = viaje.HoraLlegada,
                MaxPaquetes = viaje.MaxPaquetes,
                PesoMaxKg = viaje.PesoMaxKg,
                DesvioMaxKm = viaje.DesvioMaxKm,
                DistanciaKm = viaje.DistanciaKm,
                DuracionMin = viaje.DuracionMin,
                Activo = viaje.Activo
            };
        }

        private static List<ViajeDto> ViajesDePrueba()
        {
            return new List<ViajeDto>
            {
                new ViajeDto
                {
                    Id = 1,
                    OrigenDir = "Belgrano",
                    DestinoDir = "Lomas de Zamora",
                    Dias = new List<DiaSemana> { DiaSemana.Lunes, DiaSemana.Miercoles, DiaSemana.Viernes },
                    HoraSalida = new TimeOnly(8, 0),
                    HoraLlegada = new TimeOnly(8, 55),
                    MaxPaquetes = 3,
                    PesoMaxKg = 10,
                    DesvioMaxKm = 5,
                    DistanciaKm = 18.4m,
                    DuracionMin = 55,
                    Activo = true
                },
                new ViajeDto
                {
                    Id = 2,
                    OrigenDir = "Palermo",
                    DestinoDir = "San Isidro",
                    Dias = new List<DiaSemana> { DiaSemana.Viernes },
                    HoraSalida = new TimeOnly(18, 30),
                    HoraLlegada = new TimeOnly(19, 20),
                    MaxPaquetes = 2,
                    PesoMaxKg = 8,
                    DesvioMaxKm = 3,
                    DistanciaKm = 21.7m,
                    DuracionMin = 50,
                    Activo = true
                },
                new ViajeDto
                {
                    Id = 3,
                    OrigenDir = "Caballito",
                    DestinoDir = "Quilmes",
                    Dias = new List<DiaSemana> { DiaSemana.Martes, DiaSemana.Jueves },
                    HoraSalida = new TimeOnly(7, 15),
                    HoraLlegada = new TimeOnly(8, 10),
                    MaxPaquetes = 4,
                    PesoMaxKg = 15,
                    DesvioMaxKm = 4,
                    DistanciaKm = 24.1m,
                    DuracionMin = 55,
                    Activo = false
                }
            };
        }
    }
}
