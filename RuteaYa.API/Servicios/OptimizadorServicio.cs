using RuteaYa.API.Servicios.Interfaces;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;
using RuteaYa.Entidades;
using RuteaYa.Repositorios.Interfaces;

namespace RuteaYa.API.Servicios
{
    // Optimizador de combinaciones de envíos para un viaje.
    // Usa backtracking con poda: evalúa todas las combinaciones posibles respetando
    // las restricciones del viaje y elige la que maximiza el criterio elegido.
    // En empate de ganancia se prefiere siempre el menor desvío (y viceversa).
    public class OptimizadorServicio : IOptimizadorServicio
    {
        private static readonly TimeSpan _offsetArgentina = TimeSpan.FromHours(-3);

        private readonly IViajeRepositorio _viajeRepositorio;
        private readonly IEnvioRepositorio _envioRepositorio;
        private readonly IOportunidadesServicio _oportunidades;
        private readonly ILogger<OptimizadorServicio> _logger;

        public OptimizadorServicio(
            IViajeRepositorio viajeRepositorio,
            IEnvioRepositorio envioRepositorio,
            IOportunidadesServicio oportunidades,
            ILogger<OptimizadorServicio> logger)
        {
            _viajeRepositorio = viajeRepositorio;
            _envioRepositorio = envioRepositorio;
            _oportunidades = oportunidades;
            _logger = logger;
        }

        public async Task<OptimizacionResultadoDto?> OptimizarAsync(int usuarioId, int viajeId, string? criterio = "ganancia")
        {
            var viaje = await _viajeRepositorio.ObtenerPorIdAsync(viajeId);
            if (viaje == null || viaje.UsuarioId != usuarioId)
            {
                return null;
            }

            var criterioNorm = NormalizarCriterio(criterio);
            var hoy = Hoy();

            // La combinación tiene que entrar en el lugar que le queda al viaje (descontando lo ya aceptado).
            var (paquetesLibres, kgLibres) = OportunidadesServicio.CapacidadRestante(viaje);

            // Candidatos: envíos disponibles de otros usuarios.
            var enviosDisponibles = await _envioRepositorio.ListarDisponiblesAsync(hoy, usuarioId);

            // Evaluar cada candidato con el mismo motor de Oportunidades para no duplicar reglas.
            var candidatos = new List<CandidatoInterno>();
            var descartados = new List<EnvioDescartadoDto>();

            foreach (var envio in enviosDisponibles)
            {
                var evaluacion = await _oportunidades.EvaluarCompatibilidadAsync(viaje, envio, usuarioId, hoy);
                if (!evaluacion.EsCompatible)
                {
                    descartados.Add(new EnvioDescartadoDto
                    {
                        EnvioId = envio.Id,
                        Origen = envio.OrigenDir,
                        Destino = envio.DestinoDir,
                        Descripcion = envio.Descripcion,
                        PesoKg = envio.PesoKg,
                        DesvioKm = evaluacion.Ruta?.DesvioKm ?? 0,
                        MontoPago = envio.Monto,
                        Motivo = evaluacion.Motivo!
                    });
                    continue;
                }

                candidatos.Add(new CandidatoInterno
                {
                    Envio = envio,
                    DesvioKm = evaluacion.Ruta!.DesvioKm,
                    TiempoExtraMin = evaluacion.Ruta.TiempoAdicionalMinutos,
                    FechaViaje = evaluacion.Fecha!.Value
                });
            }

            // Ordenar candidatos para que la poda sea más efectiva: mayor ganancia primero y a igual ganancia menor desvío.
            candidatos = candidatos.OrderByDescending(c => c.Envio.Monto).ThenBy(c => c.DesvioKm).ToList();

            // Precomputar el valor máximo posible hacia adelante para poda por cota superior.
            // cotaSupGanancia[i] = suma de ganancias de candidatos[i..n-1]
            var cotaSupGanancia = new decimal[candidatos.Count + 1];
            for (var i = candidatos.Count - 1; i >= 0; i--)
            {
                cotaSupGanancia[i] = cotaSupGanancia[i + 1] + candidatos[i].Envio.Monto;
            }

            var todasLasCombinaciones = new List<CombinacionDto>();
            CombinacionDto? optima = null;
            decimal mejorValor = -1;

            // Backtracking con poda: construye combinaciones y poda las ramas que no pueden superar la mejor.
            void Backtrack(int indice, List<CandidatoInterno> actual, decimal gananciaAcum, decimal pesoAcum, decimal desvioAcum)
            {
                // Si la combinación actual no está vacía, es una candidata a guardar.
                if (actual.Count > 0)
                {
                    var comb = ConstruirCombinacion(actual, gananciaAcum, pesoAcum, desvioAcum, criterioNorm, viaje.DesvioMaxKm);
                    todasLasCombinaciones.Add(comb);

                    var valor = comb.Valor;
                    if (optima == null || EsMejor(valor, gananciaAcum, desvioAcum, mejorValor, optima.GananciaTotal, optima.DesvioTotalKm))
                    {
                        mejorValor = valor;
                        optima = comb;
                    }
                }

                for (var i = indice; i < candidatos.Count; i++)
                {
                    var c = candidatos[i];

                    // Poda 1: cantidad de paquetes agotada — todos los siguientes tampoco entran.
                    if (actual.Count + 1 > paquetesLibres)
                    {
                        break;
                    }

                    // Poda 2: este candidato supera el peso — puede haber uno más liviano adelante, seguir.
                    if (pesoAcum + c.Envio.PesoKg > kgLibres)
                    {
                        continue;
                    }

                    // Poda 3: desvío acumulado más el de este candidato supera el máximo — seguir.
                    if (desvioAcum + c.DesvioKm > viaje.DesvioMaxKm)
                    {
                        continue;
                    }

                    // Poda 4: cota superior — si incluso tomando todos los restantes con criterio ganancia
                    // es estrictamente menor que la mejor combinación hallada, cortar la rama.
                    // OJO: se usa < (no <=) porque a igual ganancia puede existir una combinación con menor desvío.
                    if (criterioNorm == "ganancia" && mejorValor >= 0)
                    {
                        var cotaMaxGanancia = gananciaAcum + cotaSupGanancia[i];
                        if (cotaMaxGanancia < mejorValor)
                        {
                            break;
                        }
                    }

                    actual.Add(c);
                    Backtrack(i + 1, actual, gananciaAcum + c.Envio.Monto, pesoAcum + c.Envio.PesoKg, desvioAcum + c.DesvioKm);
                    actual.RemoveAt(actual.Count - 1);
                }
            }

            Backtrack(0, new List<CandidatoInterno>(), 0, 0, 0);

            // Otras combinaciones: todas excepto la óptima, ordenadas por valor descendente.
            var otras = todasLasCombinaciones
                .Where(c => c != optima)
                .OrderByDescending(c => c.Valor)
                .ThenBy(c => c.DesvioTotalKm)
                .ToList();

            // Primera fecha del viaje (para mostrar en la UI; la óptima puede tener su propia FechaViaje).
            var fechaViaje = optima != null && optima.Envios.Count > 0
                ? optima.Envios[0].FechaViaje
                : candidatos.Select(c => c.FechaViaje).DefaultIfEmpty(hoy).Min();

            return new OptimizacionResultadoDto
            {
                ViajeId = viaje.Id,
                OrigenDir = viaje.OrigenDir,
                DestinoDir = viaje.DestinoDir,
                MaxPaquetes = viaje.MaxPaquetes,
                PesoMaxKg = viaje.PesoMaxKg,
                DesvioMaxKm = viaje.DesvioMaxKm,
                FechaViaje = fechaViaje,
                Criterio = criterioNorm,
                Optima = optima ?? new CombinacionDto(),
                OtrasCombinaciones = otras,
                Descartados = descartados
            };
        }

        // Construye el DTO de una combinación calculando su valor según el criterio elegido.
        private static CombinacionDto ConstruirCombinacion(
            List<CandidatoInterno> candidatos,
            decimal ganancia,
            decimal peso,
            decimal desvio,
            string criterio,
            decimal desvioMaxKm)
        {
            var valor = CalcularValor(ganancia, desvio, criterio, desvioMaxKm);
            var (compatibilidad, puntuacion) = OportunidadesServicio.CalcularCompatibilidad(desvio, desvioMaxKm);

            return new CombinacionDto
            {
                Envios = candidatos.Select(c => new OportunidadDto
                {
                    EnvioId = c.Envio.Id,
                    Origen = c.Envio.OrigenDir,
                    Destino = c.Envio.DestinoDir,
                    Descripcion = c.Envio.Descripcion,
                    PesoKg = c.Envio.PesoKg,
                    DesvioKm = c.DesvioKm,
                    MontoPago = c.Envio.Monto,
                    FechaViaje = c.FechaViaje,
                    Compatibilidad = compatibilidad,
                    PuntuacionCompatibilidad = puntuacion
                }).ToList(),
                GananciaTotal = ganancia,
                PesoTotalKg = peso,
                DesvioTotalKm = desvio,
                CantidadPaquetes = candidatos.Count,
                Valor = valor
            };
        }

        // Valor de una combinación según el criterio.
        // ganancia: suma directa de montos.
        // desvio: valor negativo del desvío — menor desvío es mejor (se compara con >=).
        // equilibrio: ganancia normalizada por el desvío — balance entre ambos.
        private static decimal CalcularValor(decimal ganancia, decimal desvio, string criterio, decimal desvioMaxKm)
        {
            return criterio switch
            {
                "desvio" => desvioMaxKm > 0 ? Math.Round(1m - desvio / desvioMaxKm, 6) : (desvio == 0 ? 1m : 0m),
                "equilibrio" => Math.Round(ganancia / (1m + desvio), 2),
                _ => ganancia // "ganancia" (default)
            };
        }

        // Determina si la combinación candidata es mejor que la actual.
        // Criterio primario: valor; desempate: mayor ganancia, luego menor desvío.
        private static bool EsMejor(
            decimal valorNuevo, decimal gananciaNueva, decimal desvioNuevo,
            decimal mejorValor, decimal mejorGanancia, decimal mejorDesvio)
        {
            if (valorNuevo > mejorValor)
            {
                return true;
            }

            if (valorNuevo == mejorValor)
            {
                if (gananciaNueva > mejorGanancia)
                {
                    return true;
                }

                if (gananciaNueva == mejorGanancia && desvioNuevo < mejorDesvio)
                {
                    return true;
                }
            }

            return false;
        }

        private static string NormalizarCriterio(string? criterio)
        {
            return criterio?.ToLowerInvariant() switch
            {
                "desvio" => "desvio",
                "equilibrio" => "equilibrio",
                _ => "ganancia"
            };
        }

        private static DateOnly Hoy()
        {
            return DateOnly.FromDateTime(DateTime.UtcNow.Add(_offsetArgentina));
        }

        // Datos de un envío candidato ya evaluado: evita recalcular desvío y fecha en el backtracking.
        private class CandidatoInterno
        {
            public Envio Envio { get; set; } = null!;
            public decimal DesvioKm { get; set; }
            public int TiempoExtraMin { get; set; }
            public DateOnly FechaViaje { get; set; }
        }
    }
}

