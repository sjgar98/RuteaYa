using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.DTOs.Rutas;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.API.Servicios
{
    public class ViajeServicio : IViajeServicio
    {
        private readonly IViajeRepositorio _repositorio;
        private readonly IRutaServicio _rutaServicio;

        public ViajeServicio(IViajeRepositorio repositorio, IRutaServicio rutaServicio)
        {
            _repositorio = repositorio;
            _rutaServicio = rutaServicio;
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
                Activo = true,
                Dias = dto.Dias.Distinct().Select(d => new ViajeDia { DiaSemana = d }).ToList()
            };
            AplicarDatos(viaje, dto);
            await CalcularRutaAsync(viaje);

            await _repositorio.AgregarAsync(viaje);

            return ADto(viaje);
        }

        // Mismas validaciones que CrearAsync (Data Annotations de CrearViajeDto).
        public async Task<ResultadoOperacion> ActualizarAsync(int usuarioId, int id, CrearViajeDto dto)
        {
            var viaje = await _repositorio.ObtenerPorIdAsync(id);

            if (viaje == null || viaje.UsuarioId != usuarioId)
            {
                return ResultadoOperacion.NoEncontrado;
            }

            // Cambiar ruta, días o capacidad dejaría inconsistentes las asignaciones en curso.
            if (TieneEnviosPendientes(viaje))
            {
                return ResultadoOperacion.Conflicto;
            }

            AplicarDatos(viaje, dto);
            await CalcularRutaAsync(viaje);

            // ViajeDia tiene clave (ViajeId, DiaSemana): borrar y volver a agregar el mismo día choca
            // en el change tracker, por eso solo se quitan y agregan las diferencias.
            var dias = dto.Dias.Distinct().ToList();
            viaje.Dias.RemoveAll(d => !dias.Contains(d.DiaSemana));
            viaje.Dias.AddRange(dias
                .Where(d => viaje.Dias.All(existente => existente.DiaSemana != d))
                .Select(d => new ViajeDia { DiaSemana = d }));

            await _repositorio.GuardarCambiosAsync();

            return ResultadoOperacion.Exito;
        }

        public async Task<ResultadoOperacion> CambiarActivoAsync(int usuarioId, int id, bool activo)
        {
            var viaje = await _repositorio.ObtenerPorIdAsync(id);

            if (viaje == null || viaje.UsuarioId != usuarioId)
            {
                return ResultadoOperacion.NoEncontrado;
            }

            // Un viaje inactivo no recibe oportunidades: dejaría sin conductor a los envíos en curso.
            if (!activo && TieneEnviosPendientes(viaje))
            {
                return ResultadoOperacion.Conflicto;
            }

            viaje.Activo = activo;
            await _repositorio.GuardarCambiosAsync();

            return ResultadoOperacion.Exito;
        }


        // Datos del formulario comunes a crear y editar (los días se tratan aparte).
        private static void AplicarDatos(Viaje viaje, CrearViajeDto dto)
        {
            viaje.OrigenDir = dto.OrigenDir.Trim();
            viaje.OrigenLat = dto.OrigenLat!.Value;
            viaje.OrigenLng = dto.OrigenLng!.Value;
            viaje.DestinoDir = dto.DestinoDir.Trim();
            viaje.DestinoLat = dto.DestinoLat!.Value;
            viaje.DestinoLng = dto.DestinoLng!.Value;
            viaje.HoraSalida = dto.HoraSalida!.Value;
            viaje.HoraLlegada = dto.HoraLlegada!.Value;
            viaje.MaxPaquetes = dto.MaxPaquetes!.Value;
            viaje.PesoMaxKg = dto.PesoMaxKg!.Value;
            viaje.DesvioMaxKm = dto.DesvioMaxKm!.Value;

            // Las paradas que se quitan de la colección las borra EF (FK obligatoria con cascada).
            viaje.Paradas.Clear();
            viaje.Paradas.AddRange(dto.Paradas.Select((p, i) => new Parada
            {
                Orden = i + 1,
                Direccion = p.Direccion.Trim(),
                Lat = p.Lat,
                Lng = p.Lng
            }));
        }

        private async Task CalcularRutaAsync(Viaje viaje)
        {
            var recorrido = new List<CoordenadaDto> { new CoordenadaDto(viaje.OrigenLat, viaje.OrigenLng) };
            recorrido.AddRange(viaje.Paradas.OrderBy(p => p.Orden).Select(p => new CoordenadaDto(p.Lat, p.Lng)));
            recorrido.Add(new CoordenadaDto(viaje.DestinoLat, viaje.DestinoLng));

            var ruta = await _rutaServicio.CalcularRutaAsync(recorrido);
            viaje.DistanciaKm = ruta.DistanciaKm;
            viaje.DuracionMin = ruta.DuracionMinutos;
        }



        private static bool TieneEnviosPendientes(Viaje viaje)
        {
            return viaje.Asignaciones.Any(a => a.Envio.Estado == EstadoEnvio.ConductorAsignado
                || a.Envio.Estado == EstadoEnvio.Retirado
                || a.Envio.Estado == EstadoEnvio.EnCamino);
        }

        private static ViajeDto ADto(Viaje viaje)
        {
            return CompletarDatos(viaje, new ViajeDto());
        }

        private static ViajeDetalleDto ADetalleDto(Viaje viaje)
        {
            var dto = CompletarDatos(viaje, new ViajeDetalleDto());
            dto.OrigenLat = viaje.OrigenLat;
            dto.OrigenLng = viaje.OrigenLng;
            dto.DestinoLat = viaje.DestinoLat;
            dto.DestinoLng = viaje.DestinoLng;

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
            dto.TieneEnviosPendientes = TieneEnviosPendientes(viaje);
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

    }
}
