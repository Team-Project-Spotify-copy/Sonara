using System.ComponentModel.DataAnnotations;

namespace Application.Validators;

/// <summary>
/// Rejects Guid.Empty, which model binding otherwise accepts as a valid value
/// for a non-nullable Guid (and which can never identify a real entity).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public NotEmptyGuidAttribute()
        : base("The {0} field must be a non-empty identifier.")
    {
    }

    public override bool IsValid(object? value) => value switch
    {
        null => true, // nullability is [Required]'s concern
        Guid guid => guid != Guid.Empty,
        _ => false
    };
}
