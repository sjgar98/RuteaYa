using System.ComponentModel.DataAnnotations;

namespace RuteaYa.DTOs.Validaciones
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FechaNoAnteriorAHoyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateOnly fecha && fecha < DateOnly.FromDateTime(DateTime.Today))
            {
                return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
            }

            return ValidationResult.Success;
        }
    }
}