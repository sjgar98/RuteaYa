using System.ComponentModel.DataAnnotations;

namespace RuteaYa.DTOs.Validaciones
{
    // Una lista de direcciones sin repetidas y que no coincidan con las de las propiedades indicadas.
    [AttributeUsage(AttributeTargets.Property)]
    public class DireccionesUnicasAttribute : ValidationAttribute
    {
        private readonly string[] _propiedadesExcluidas;

        public DireccionesUnicasAttribute(params string[] propiedadesExcluidas)
        {
            _propiedadesExcluidas = propiedadesExcluidas;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not IEnumerable<DireccionDto> direcciones)
            {
                return ValidationResult.Success;
            }

            var usadas = _propiedadesExcluidas
                .Select(p => validationContext.ObjectType.GetProperty(p)?.GetValue(validationContext.ObjectInstance) as string)
                .ToList();

            foreach (var direccion in direcciones)
            {
                if (usadas.Any(u => DistintoDeAttribute.TextoIgual(direccion.Direccion, u)))
                {
                    return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
                }

                usadas.Add(direccion.Direccion);
            }

            return ValidationResult.Success;
        }
    }
}
