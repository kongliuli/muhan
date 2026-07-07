using System;
using System.Globalization;
using System.Windows.Controls;

namespace ModernBoxes.Presentation.Validation
{
    public class IsEmptyValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty(value?.ToString()))
            {
                return new ValidationResult(false, "²»ÄÜÎª¿Õ");
            }
            return new ValidationResult(true, "");
        }
    }
}