namespace cCoder.Core.Objects.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ApiIgnoreOperationAttribute : Attribute
{
    public string Operation { get; set; }

    public ApiIgnoreOperationAttribute(string op) { Operation = op; }
}
