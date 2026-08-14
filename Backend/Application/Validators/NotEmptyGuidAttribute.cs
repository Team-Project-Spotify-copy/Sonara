using System.ComponentModel.DataAnnotations;

namespace Application.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public NotEmptyGuidAttribute()
        : base("The {0} field must be a non-empty identifier.")
    {
    }

    public override bool IsValid(object? value) => value switch
    {
        null => true,
        Guid guid => guid != Guid.Empty,
        _ => false
    };
}
