using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.API.Servicios
{
    public class EnvioServicio : IEnvioServicio
    {
        private readonly IEnvioRepositorio _repositorio;

        public EnvioServicio(IEnvioRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<EnvioDto>> ListarPorUsuarioAsync(int usuarioId, EstadoEnvio? estado)
        {
            var envios = await _repositorio.ListarPorUsuarioAsync(usuarioId, estado);
            return envios.Select(ADto).ToList();
        }

        public async Task<EnvioDetalleDto?> ObtenerDetalleAsync(int usuarioId, int id)
        {
            var envio = await _repositorio.ObtenerPorIdAsync(id);

            // Un envío ajeno se trata como inexistente para no revelar que existe.
            if (envio == null || envio.UsuarioId != usuarioId)
            {
                return null;
            }

            return ADetalleDto(envio);
        }

        public async Task<ViajeDetalleDto?> ObtenerViajeAsociadoAsync(int usuarioId, int envioId)
        {
            var envio = await _repositorio.ObtenerPorIdAsync(envioId);

            if (envio == null || envio.UsuarioId != usuarioId || envio.Asignacion?.Viaje == null)
            {
                return null;
            }

            return AViajeDetalleDto(envio.Asignacion.Viaje);
        }

        public async Task<ResultadoOperacion> CancelarAsync(int usuarioId, int id)
        {
            var envio = await _repositorio.ObtenerPorIdAsync(id);

            if (envio == null || envio.UsuarioId != usuarioId)
            {
                return ResultadoOperacion.NoEncontrado;
            }

            if (!EnvioEstadoMaquina.Transicionar(envio, EstadoEnvio.Cancelado))
            {
                return ResultadoOperacion.Conflicto;
            }

            await _repositorio.GuardarCambiosAsync();

            return ResultadoOperacion.Exito;
        }
        public async Task<EnvioDto> CrearAsync(int usuarioId, CrearEnvioDto dto)
        {
            var envio = new Envio
            {
                UsuarioId = usuarioId,
                CreadoEn = DateTime.UtcNow
            };
            AplicarDatos(envio, dto);

            envio.Historial.Add(new EnvioEstadoHistorial
            {
                EstadoAnterior = null,
                EstadoNuevo = envio.Estado,
                Fecha = envio.CreadoEn
            });

            await _repositorio.AgregarAsync(envio);

            return ADto(envio);
        }

        private static EnvioDto ADto(Envio envio)
        {
            return CompletarDatos(envio, new EnvioDto());
        }

        private static void AplicarDatos(Envio envio, CrearEnvioDto dto)
        {
            envio.OrigenDir = dto.OrigenDir.Trim();
            envio.OrigenLat = dto.OrigenLat!.Value;
            envio.OrigenLng = dto.OrigenLng!.Value;
            envio.DestinoDir = dto.DestinoDir.Trim();
            envio.DestinoLat = dto.DestinoLat!.Value;
            envio.DestinoLng = dto.DestinoLng!.Value;
            envio.FechaLimite = dto.FechaLimite!.Value;
            envio.PesoKg = dto.PesoKg!.Value;
            envio.Tamano = dto.Tamano!.Value;
            envio.Descripcion = dto.Descripcion.Trim();
            envio.Monto = dto.Monto!.Value;
        }

        private static EnvioDetalleDto ADetalleDto(Envio envio)
        {
            var dto = CompletarDatos(envio, new EnvioDetalleDto());
            dto.OrigenLat = envio.OrigenLat;
            dto.OrigenLng = envio.OrigenLng;
            dto.DestinoLat = envio.DestinoLat;
            dto.DestinoLng = envio.DestinoLng;

            if (envio.Asignacion != null)
            {
                dto.ViajeAsociado = new ViajeAsociadoDto
                {
                    ViajeId = envio.Asignacion.ViajeId,
                    OrigenDir = envio.Asignacion.Viaje.OrigenDir,
                    DestinoDir = envio.Asignacion.Viaje.DestinoDir,
                    FechaViaje = envio.Asignacion.FechaViaje,
                    DesvioKm = envio.Asignacion.DesvioKm,
                    Ganancia = envio.Asignacion.Ganancia
                };
            }

            dto.Historial = envio.Historial
                .Select(h => new EnvioHistorialDto
                {
                    EstadoAnterior = h.EstadoAnterior,
                    EstadoNuevo = h.EstadoNuevo,
                    Fecha = h.Fecha
                })
                .ToList();

            return dto;
        }

        private static ViajeDetalleDto AViajeDetalleDto(Viaje viaje)
        {
            var dto = new ViajeDetalleDto
            {
                Id = viaje.Id,
                OrigenDir = viaje.OrigenDir,
                DestinoDir = viaje.DestinoDir,
                OrigenLat = viaje.OrigenLat,
                OrigenLng = viaje.OrigenLng,
                DestinoLat = viaje.DestinoLat,
                DestinoLng = viaje.DestinoLng,
                Dias = viaje.Dias.Select(d => d.DiaSemana).OrderBy(d => d).ToList(),
                HoraSalida = viaje.HoraSalida,
                HoraLlegada = viaje.HoraLlegada,
                MaxPaquetes = viaje.MaxPaquetes,
                PesoMaxKg = viaje.PesoMaxKg,
                DesvioMaxKm = viaje.DesvioMaxKm,
                DistanciaKm = viaje.DistanciaKm,
                DuracionMin = viaje.DuracionMin,
                Activo = viaje.Activo,
                TieneEnviosPendientes = viaje.Asignaciones.Any(a => a.Envio.Estado == EstadoEnvio.ConductorAsignado
                    || a.Envio.Estado == EstadoEnvio.Retirado
                    || a.Envio.Estado == EstadoEnvio.EnCamino),
                Paradas = viaje.Paradas
                    .OrderBy(p => p.Orden)
                    .Select(p => new DireccionDto { Direccion = p.Direccion, Lat = p.Lat, Lng = p.Lng })
                    .ToList(),
                PaquetesAsignados = viaje.Asignaciones
                    .OrderBy(a => a.FechaViaje)
                    .ThenBy(a => a.EnvioId)
                    .Select(a => new PaqueteAsignadoDto
                    {
                        EnvioId = a.EnvioId,
                        Origen = a.Envio.OrigenDir,
                        Destino = a.Envio.DestinoDir,
                        Descripcion = a.Envio.Descripcion,
                        Estado = a.Envio.Estado,
                        FechaViaje = a.FechaViaje,
                        FechaLimite = a.Envio.FechaLimite,
                        PesoKg = a.Envio.PesoKg,
                        Ganancia = a.Ganancia
                    })
                    .ToList()
            };

            return dto;
        }

        private static T CompletarDatos<T>(Envio envio, T dto) where T : EnvioDto
        {
            dto.Id = envio.Id;
            dto.OrigenDir = envio.OrigenDir;
            dto.DestinoDir = envio.DestinoDir;
            dto.FechaLimite = envio.FechaLimite;
            dto.PesoKg = envio.PesoKg;
            dto.Tamano = envio.Tamano;
            dto.Descripcion = envio.Descripcion;
            dto.Monto = envio.Monto;
            dto.Estado = envio.Estado;
            dto.PuedeCancelar = EnvioEstadoMaquina.PuedeTransicionar(envio.Estado, EstadoEnvio.Cancelado);
            return dto;
        }

    }
}