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

        public async Task<List<ViajeDto>> ListarPorUsuarioAsync(int usuarioId, bool? activo)
        {
            var viajes = await _repositorio.ListarPorUsuarioAsync(usuarioId, activo);
            return viajes.Select(ADto).ToList();
        }

        public async Task<ViajeDetalleDto?> ObtenerDetalleAsync(int usuarioId, int id)
        {
            var viaje = await _repositorio.ObtenerPorIdAsync(id);

            // Un viaje ajeno se trata como inexistente para no revelar que existe.
            if (viaje == null || viaje.UsuarioId != usuarioId)
            {
                return null;
            }

            return ADetalleDto(viaje);
        }

        // Las validaciones de los datos están en las Data Annotations de CrearViajeDto.
        public async Task<ViajeDto> CrearAsync(int usuarioId, CrearViajeDto dto)
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

            await _repositorio.AgregarAsync(viaje);

            return ADto(viaje);
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
            return CompletarDatos(viaje, new ViajeDto());
        }

        private static ViajeDetalleDto ADetalleDto(Viaje viaje)
        {
            var dto = CompletarDatos(viaje, new ViajeDetalleDto());

            dto.Paradas = viaje.Paradas
                .OrderBy(p => p.Orden)
                .Select(p => new DireccionDto { Direccion = p.Direccion, Lat = p.Lat, Lng = p.Lng })
                .ToList();

            // TODO Paquetes asignados: mapear viaje.Asignaciones (con su Envio) cuando existan envíos y asignaciones.
            dto.PaquetesAsignados = PaquetesDePrueba();

            return dto;
        }

        private static T CompletarDatos<T>(Viaje viaje, T dto) where T : ViajeDto
        {
            dto.Id = viaje.Id;
            dto.OrigenDir = viaje.OrigenDir;
            dto.DestinoDir = viaje.DestinoDir;
            dto.Dias = viaje.Dias.Select(d => d.DiaSemana).OrderBy(d => d).ToList();
            dto.HoraSalida = viaje.HoraSalida;
            dto.HoraLlegada = viaje.HoraLlegada;
            dto.MaxPaquetes = viaje.MaxPaquetes;
            dto.PesoMaxKg = viaje.PesoMaxKg;
            dto.DesvioMaxKm = viaje.DesvioMaxKm;
            dto.DistanciaKm = viaje.DistanciaKm;
            dto.DuracionMin = viaje.DuracionMin;
            dto.Activo = viaje.Activo;
            return dto;
        }

        private static List<PaqueteAsignadoDto> PaquetesDePrueba()
        {
            return new List<PaqueteAsignadoDto>
            {
                new PaqueteAsignadoDto
                {
                    EnvioId = 1,
                    Descripcion = "Zapatillas",
                    Estado = EstadoEnvio.Retirado,
                    FechaLimite = new DateOnly(2026, 11, 20),
                    Ganancia = 4000m
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
