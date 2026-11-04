using System.ComponentModel.DataAnnotations;
using RuteaYa.DTOs;
using RuteaYa.DTOs.Enums;

namespace RuteaYa.Tests
{
    public class ValidacionesDtoTests
    {
        [Fact]
        public void CrearViajeDto_Valido_NoTieneErrores()
        {
            Assert.Empty(Validar(ViajeValido()));
        }

        [Fact]
        public void CrearViajeDto_DestinoIgualAlOrigen_EsInvalido()
        {
            var dto = ViajeValido();
            dto.DestinoDir = dto.OrigenDir;

            Assert.Contains(nameof(CrearViajeDto.DestinoDir), CamposConError(dto));
        }

        [Fact]
        public void CrearViajeDto_SinDias_EsInvalido()
        {
            var dto = ViajeValido();
            dto.Dias.Clear();

            Assert.Contains(nameof(CrearViajeDto.Dias), CamposConError(dto));
        }

        [Theory]
        [InlineData(8, 0)]
        [InlineData(7, 30)]
        public void CrearViajeDto_LlegadaNoPosteriorALaSalida_EsInvalido(int hora, int minuto)
        {
            var dto = ViajeValido();
            dto.HoraLlegada = new TimeOnly(hora, minuto);

            Assert.Contains(nameof(CrearViajeDto.HoraLlegada), CamposConError(dto));
        }

        [Fact]
        public void CrearViajeDto_ParadasRepetidas_EsInvalido()
        {
            var dto = ViajeValido();
            dto.Paradas.Add(new DireccionDto { Direccion = "Caballito, CABA", Lat = -34.6189m, Lng = -58.4422m });
            dto.Paradas.Add(new DireccionDto { Direccion = "Caballito, CABA", Lat = -34.6189m, Lng = -58.4422m });

            Assert.Contains(nameof(CrearViajeDto.Paradas), CamposConError(dto));
        }

        [Fact]
        public void CrearViajeDto_ParadaIgualAlOrigen_EsInvalido()
        {
            var dto = ViajeValido();
            dto.Paradas.Add(new DireccionDto { Direccion = dto.OrigenDir, Lat = -34.5627m, Lng = -58.4565m });

            Assert.Contains(nameof(CrearViajeDto.Paradas), CamposConError(dto));
        }

        [Fact]
        public void CrearViajeDto_LimitesEnCero_SonInvalidos()
        {
            var dto = ViajeValido();
            dto.MaxPaquetes = 0;
            dto.PesoMaxKg = 0m;
            dto.DesvioMaxKm = 0m;

            var campos = CamposConError(dto);

            Assert.Contains(nameof(CrearViajeDto.MaxPaquetes), campos);
            Assert.Contains(nameof(CrearViajeDto.PesoMaxKg), campos);
            Assert.Contains(nameof(CrearViajeDto.DesvioMaxKm), campos);
        }

        [Fact]
        public void CrearViajeDto_SinCoordenadas_EsInvalido()
        {
            var dto = ViajeValido();
            dto.OrigenLat = null;

            Assert.Contains(nameof(CrearViajeDto.OrigenLat), CamposConError(dto));
        }

        [Fact]
        public void CrearEnvioDto_Valido_NoTieneErrores()
        {
            Assert.Empty(Validar(EnvioValido()));
        }

        [Fact]
        public void CrearEnvioDto_DestinoIgualAlOrigen_EsInvalido()
        {
            var dto = EnvioValido();
            dto.DestinoDir = dto.OrigenDir;

            Assert.Contains(nameof(CrearEnvioDto.DestinoDir), CamposConError(dto));
        }

        [Fact]
        public void CrearEnvioDto_FechaAnteriorAHoy_EsInvalida()
        {
            var dto = EnvioValido();
            dto.FechaLimite = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);

            Assert.Contains(nameof(CrearEnvioDto.FechaLimite), CamposConError(dto));
        }

        [Fact]
        public void CrearEnvioDto_PesoYMontoEnCero_SonInvalidos()
        {
            var dto = EnvioValido();
            dto.PesoKg = 0m;
            dto.Monto = 0m;

            var campos = CamposConError(dto);

            Assert.Contains(nameof(CrearEnvioDto.PesoKg), campos);
            Assert.Contains(nameof(CrearEnvioDto.Monto), campos);
        }

        [Fact]
        public void CrearEnvioDto_SinTamanoNiDescripcion_EsInvalido()
        {
            var dto = EnvioValido();
            dto.Tamano = null;
            dto.Descripcion = string.Empty;

            var campos = CamposConError(dto);

            Assert.Contains(nameof(CrearEnvioDto.Tamano), campos);
            Assert.Contains(nameof(CrearEnvioDto.Descripcion), campos);
        }

        private static List<ValidationResult> Validar(object dto)
        {
            var resultados = new List<ValidationResult>();
            Validator.TryValidateObject(dto, new ValidationContext(dto), resultados, validateAllProperties: true);
            return resultados;
        }

        private static List<string> CamposConError(object dto)
        {
            return Validar(dto).SelectMany(r => r.MemberNames).ToList();
        }

        private static CrearViajeDto ViajeValido()
        {
            return new CrearViajeDto
            {
                OrigenDir = "Belgrano, CABA",
                OrigenLat = -34.5627m,
                OrigenLng = -58.4565m,
                DestinoDir = "Lomas de Zamora, Buenos Aires",
                DestinoLat = -34.7609m,
                DestinoLng = -58.4063m,
                Dias = new List<DiaSemana> { DiaSemana.Lunes, DiaSemana.Miercoles },
                HoraSalida = new TimeOnly(8, 0),
                HoraLlegada = new TimeOnly(8, 55),
                MaxPaquetes = 3,
                PesoMaxKg = 10m,
                DesvioMaxKm = 5m
            };
        }

        private static CrearEnvioDto EnvioValido()
        {
            return new CrearEnvioDto
            {
                OrigenDir = "Caballito, CABA",
                OrigenLat = -34.6189m,
                OrigenLng = -58.4422m,
                DestinoDir = "Lomas de Zamora, Buenos Aires",
                DestinoLat = -34.7609m,
                DestinoLng = -58.4063m,
                FechaLimite = DateOnly.FromDateTime(DateTime.Today).AddDays(7),
                PesoKg = 2m,
                Tamano = TamanoEnvio.Pequeno,
                Descripcion = "Caja con zapatillas",
                Monto = 4000m
            };
        }
    }
}
