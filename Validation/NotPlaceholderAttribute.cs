using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Validation;

/// <summary>
/// Rejects the literal value "string" (Swagger's auto-filled placeholder).
/// Allows null/empty — combine with [Required] if the field is mandatory.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class NotPlaceholderAttribute : ValidationAttribute
{
    private static readonly HashSet<string> Placeholders = new(StringComparer.OrdinalIgnoreCase)
    {
        "string", "String"
    };

    public NotPlaceholderAttribute()
        : base("'{0}' must not be the placeholder value \"string\". Provide a real value or leave it empty.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is string str && Placeholders.Contains(str))
        {
            return new ValidationResult(
                FormatErrorMessage(context.DisplayName),
                new[] { context.MemberName ?? context.DisplayName });
        }

        return ValidationResult.Success;
    }
}
