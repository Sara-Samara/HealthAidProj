// Helpers/ValidationAttributes.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.Helpers
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date < DateTime.Today)
                {
                    return new ValidationResult("Date must be in the future");
                }
            }
            return ValidationResult.Success;
        }
    }

    public class DateAfterAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateAfterAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var currentValue = value as DateTime?;

            if (currentValue.HasValue)
            {
                var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
                if (property == null)
                    throw new ArgumentException("Property with this name not found");

                var comparisonValue = property.GetValue(validationContext.ObjectInstance) as DateTime?;

                if (comparisonValue.HasValue && currentValue <= comparisonValue)
                {
                    return new ValidationResult($"End date must be after {_comparisonProperty}");
                }
            }

            return ValidationResult.Success;
        }
    }
}