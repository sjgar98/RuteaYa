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