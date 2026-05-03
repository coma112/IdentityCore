using System.ComponentModel.DataAnnotations;

namespace IdentityCore.Attributes
{
    public class MinDecimalAttribute : ValidationAttribute
    {
        private readonly decimal _minimum;

        public MinDecimalAttribute(double minimum)
        {
            _minimum = (decimal)minimum;
            ErrorMessage = $"Amount must be at least {_minimum}.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is decimal amount && amount >= _minimum)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}
