using System;

namespace cCoder.Core.Objects
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ApiReadOnlyAttribute : Attribute
    {
        public string Operation { get; set; }
    }
}
