namespace cCoder.Core.Objects.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ApiReadOnlyAttribute : Attribute
{
    public string Operation { get; set; }
}
