using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.API.Servicios
{
    public class TransporteServicio : ITransporteServicio
    {
        private readonly IAsignacionRepositorio _repositorio;

        public TransporteServicio(IAsignacionRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<TransporteDto>> ListarAsync(int conductorId)
        {
            var asignaciones = await _repositorio.ListarPorConductorAsync(conductorId);
            return asignaciones.Select(ADto).ToList();
        }

        public async Task<EnvioDetalleDto?> ObtenerEnvioAsync(int conductorId, int asignacionId)
        {
            var asignacion = await _repositorio.ObtenerPorIdAsync(asignacionId);

            if (asignacion == null || asignacion.Viaje.UsuarioId != conductorId)
            {
                return null;
            }

            var envio = asignacion.Envio;
            return new EnvioDetalleDto
            {
                Id = envio.Id,
                OrigenDir = envio.OrigenDir,
                DestinoDir = envio.DestinoDir,
                OrigenLat = envio.OrigenLat,
                OrigenLng = envio.OrigenLng,
                DestinoLat = envio.DestinoLat,
                DestinoLng = envio.DestinoLng,
                FechaLimite = envio.FechaLimite,
                PesoKg = envio.PesoKg,
                Tamano = envio.Tamano,
                Descripcion = envio.Descripcion,
                Monto = envio.Monto,
                Estado = envio.Estado,
                PuedeCancelar = false,
                ViajeAsociado = new ViajeAsociadoDto
                {
                    ViajeId = asignacion.ViajeId,
                    OrigenDir = asignacion.Viaje.OrigenDir,
                    DestinoDir = asignacion.Viaje.DestinoDir,
                    FechaViaje = asignacion.FechaViaje,
                    DesvioKm = asignacion.DesvioKm,
                    Ganancia = asignacion.Ganancia
                },
                Historial = envio.Historial.Select(h => new EnvioHistorialDto
                {
                    EstadoAnterior = h.EstadoAnterior,
                    EstadoNuevo = h.EstadoNuevo,
                    Fecha = h.Fecha
                }).ToList()
            };
        }

        public Task<ResultadoOperacion> MarcarRetiradoAsync(int conductorId, int asignacionId)
        {
            return CambiarEstadoAsync(conductorId, asignacionId, EstadoEnvio.Retirado);
        }

        public Task<ResultadoOperacion> MarcarEnCaminoAsync(int conductorId, int asignacionId)
        {
            return CambiarEstadoAsync(conductorId, asignacionId, EstadoEnvio.EnCamino);
        }

        public Task<ResultadoOperacion> MarcarEntregadoAsync(int conductorId, int asignacionId)
        {
            return CambiarEstadoAsync(conductorId, asignacionId, EstadoEnvio.Entregado);
        }

        private async Task<ResultadoOperacion> CambiarEstadoAsync(int conductorId, int asignacionId, EstadoEnvio nuevoEstado)
        {
            var asignacion = await _repositorio.ObtenerPorIdAsync(asignacionId);

            if (asignacion == null || asignacion.Viaje.UsuarioId != conductorId)
            {
                return ResultadoOperacion.NoEncontrado;
            }

            if (!EnvioEstadoMaquina.Transicionar(asignacion.Envio, nuevoEstado))
            {
                return ResultadoOperacion.Conflicto;
            }

            await _repositorio.GuardarCambiosAsync();

            return ResultadoOperacion.Exito;
        }

        private static TransporteDto ADto(Asignacion asignacion)
        {
            var estado = asignacion.Envio.Estado;

            return new TransporteDto
            {
                AsignacionId = asignacion.Id,
                EnvioId = asignacion.EnvioId,
                ViajeId = asignacion.ViajeId,
                Descripcion = asignacion.Envio.Descripcion,
                OrigenDir = asignacion.Envio.OrigenDir,
                DestinoDir = asignacion.Envio.DestinoDir,
                Estado = estado,
                Ganancia = asignacion.Ganancia,
                FechaViaje = asignacion.FechaViaje,
                ViajeOrigenDir = asignacion.Viaje.OrigenDir,
                ViajeDestinoDir = asignacion.Viaje.DestinoDir,
                PuedeMarcarRetirado = EnvioEstadoMaquina.PuedeTransicionar(estado, EstadoEnvio.Retirado),
                PuedeMarcarEnCamino = EnvioEstadoMaquina.PuedeTransicionar(estado, EstadoEnvio.EnCamino),
                PuedeMarcarEntregado = EnvioEstadoMaquina.PuedeTransicionar(estado, EstadoEnvio.Entregado)
            };
        }
    }
}