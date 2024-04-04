using System;

namespace cCoder.Core.Objects
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ApiIgnoreOperationAttribute : Attribute
    {
        public string Operation { get; set; }

        public ApiIgnoreOperationAttribute(string op) { Operation = op; }
    }
}
