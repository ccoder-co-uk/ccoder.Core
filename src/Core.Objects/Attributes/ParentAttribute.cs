using System;

namespace Core.Objects
{
    // Security Attributes
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ParentAttribute : Attribute
    {
        public string PropertyName { get; set; }

        public ParentAttribute(string prop) { PropertyName = prop; }
    }
}
