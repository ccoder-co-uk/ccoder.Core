namespace cCoder.Core.Objects.Attributes;

// Metadata Attributes
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public sealed class SwaggerExcludeAttribute : Attribute { }
