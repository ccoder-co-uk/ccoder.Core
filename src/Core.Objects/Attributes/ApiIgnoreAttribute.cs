using System;

namespace Core.Objects
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class ApiIgnoreAttribute : Attribute { }
}
