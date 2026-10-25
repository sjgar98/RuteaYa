using System.ComponentModel.DataAnnotations;

namespace RuteaYa.DTOs.Validaciones
{
    // El valor debe ser mayor que el de otra propiedad del mismo tipo. Si alguno falta, no valida (lo cubre [Required]).
    [AttributeUsage(AttributeTargets.Property)]
    public class MayorQueAttribute : ValidationAttribute
    {
        private readonly string _otraPropiedad;

        public MayorQueAttribute(string otraPropiedad)
        {
            _otraPropiedad = otraPropiedad;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var otroValor = validationContext.ObjectType.GetProperty(_otraPropiedad)?.GetValue(validationContext.ObjectInstance);

            if (value is IComparable valor && otroValor != null && valor.CompareTo(otroValor) <= 0)
            {
                return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
            }

            return ValidationResult.Success;
        }
    }
}
