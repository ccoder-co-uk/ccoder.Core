using System;

namespace cCoder.Core.Objects
{
    // Metadata Attributes
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class SwaggerExcludeAttribute : Attribute { }
}
