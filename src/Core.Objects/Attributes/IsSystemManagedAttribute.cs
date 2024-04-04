using System;

namespace cCoder.Core.Objects
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class IsSystemManagedAttribute : Attribute { }
}
