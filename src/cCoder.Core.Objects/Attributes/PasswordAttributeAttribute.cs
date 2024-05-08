using System.ComponentModel.DataAnnotations;

namespace cCoder.Core.Objects.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public sealed class PasswordAttributeAttribute : ValidationAttribute
{
    public override bool RequiresValidationContext => false;

    public override bool IsValid(object value)
        => value is string s &&
            s.Any(c => char.IsNumber(c)) &&
            s.Any(c => char.IsLetter(c)) &&
            s.Any(c => char.IsUpper(c)) &&
            s.Any(c => char.IsLower(c)) &&
            s.Any(c => !char.IsUpper(c) && !char.IsLower(c) && !char.IsUpper(c) && !char.IsLetter(c) && !char.IsNumber(c));
}
