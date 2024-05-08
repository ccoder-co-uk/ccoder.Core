namespace cCoder.Core.Objects.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public sealed class IsSystemManagedAttribute : Attribute { }
