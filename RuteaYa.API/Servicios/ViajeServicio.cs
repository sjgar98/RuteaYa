using System.Globalization;
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

        // Las validaciones de los datos están en las Data Annotations de CrearViajeDto.
        public Task<ViajeDto> CrearAsync(int usuarioId, CrearViajeDto dto)
        {
            var viaje = new Viaje
            {
                UsuarioId = usuarioId,
                OrigenDir = dto.OrigenDir.Trim(),
                OrigenLat = dto.OrigenLat!.Value,
                OrigenLng = dto.OrigenLng!.Value,
                DestinoDir = dto.DestinoDir.Trim(),
                DestinoLat = dto.DestinoLat!.Value,
                DestinoLng = dto.DestinoLng!.Value,
                HoraSalida = dto.HoraSalida!.Value,
                HoraLlegada = dto.HoraLlegada!.Value,
                MaxPaquetes = dto.MaxPaquetes!.Value,
                PesoMaxKg = dto.PesoMaxKg!.Value,
                DesvioMaxKm = dto.DesvioMaxKm!.Value,
                // TODO Servicio de rutas: calcular distancia y duración de la ruta habitual (con paradas).
                DistanciaKm = 0,
                DuracionMin = 0,
                Activo = true,
                Dias = dto.Dias.Distinct().Select(d => new ViajeDia { DiaSemana = d }).ToList(),
                Paradas = dto.Paradas.Select((p, i) => new Parada
                {
                    Orden = i + 1,
                    Direccion = p.Direccion.Trim(),
                    Lat = p.Lat,
                    Lng = p.Lng
                }).ToList()
            };

            // TODO Datos de prueba: persistir cuando haya usuarios cargados en la base (y marcar el método como async).
            // await _repositorio.AgregarAsync(viaje);

            return Task.FromResult(ADto(viaje));
        }

        public Task<List<DireccionDto>> BuscarDireccionesAsync(string texto)
        {
            // TODO Servicio de mapas: reemplazar la lista fija por la búsqueda del proveedor de mapas.
            var direcciones = DireccionesDePrueba()
                .Where(d => CultureInfo.InvariantCulture.CompareInfo.IndexOf(
                    d.Direccion, texto.Trim(), CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)
                .ToList();

            return Task.FromResult(direcciones);
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

        private static List<DireccionDto> DireccionesDePrueba()
        {
            return new List<DireccionDto>
            {
                new DireccionDto { Direccion = "Belgrano, CABA", Lat = -34.562700m, Lng = -58.456500m },
                new DireccionDto { Direccion = "Palermo, CABA", Lat = -34.588900m, Lng = -58.430600m },
                new DireccionDto { Direccion = "Plaza Italia, Palermo, CABA", Lat = -34.581000m, Lng = -58.420800m },
                new DireccionDto { Direccion = "Recoleta, CABA", Lat = -34.587500m, Lng = -58.397400m },
                new DireccionDto { Direccion = "Obelisco, CABA", Lat = -34.603700m, Lng = -58.381600m },
                new DireccionDto { Direccion = "Caballito, CABA", Lat = -34.618900m, Lng = -58.442200m },
                new DireccionDto { Direccion = "Barracas, CABA", Lat = -34.645300m, Lng = -58.382600m },
                new DireccionDto { Direccion = "Avellaneda, Buenos Aires", Lat = -34.662700m, Lng = -58.365300m },
                new DireccionDto { Direccion = "Lomas de Zamora, Buenos Aires", Lat = -34.760900m, Lng = -58.406300m },
                new DireccionDto { Direccion = "Quilmes, Buenos Aires", Lat = -34.720600m, Lng = -58.254600m },
                new DireccionDto { Direccion = "Morón, Buenos Aires", Lat = -34.653400m, Lng = -58.619800m },
                new DireccionDto { Direccion = "San Isidro, Buenos Aires", Lat = -34.470800m, Lng = -58.528600m }
            };
        }
    }
}
