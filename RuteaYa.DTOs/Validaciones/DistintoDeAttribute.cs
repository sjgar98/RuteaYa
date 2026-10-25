using System.ComponentModel.DataAnnotations;

namespace RuteaYa.DTOs.Validaciones
{
    // El texto no puede coincidir con el de otra propiedad (sin distinguir mayúsculas ni espacios en los extremos).
    [AttributeUsage(AttributeTargets.Property)]
    public class DistintoDeAttribute : ValidationAttribute
    {
        private readonly string _otraPropiedad;

        public DistintoDeAttribute(string otraPropiedad)
        {
            _otraPropiedad = otraPropiedad;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var otroValor = validationContext.ObjectType.GetProperty(_otraPropiedad)?.GetValue(validationContext.ObjectInstance) as string;

            if (value is string texto && TextoIgual(texto, otroValor))
            {
                return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
            }

            return ValidationResult.Success;
        }

        internal static bool TextoIgual(string? a, string? b)
        {
            return !string.IsNullOrWhiteSpace(a)
                && string.Equals(a.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
