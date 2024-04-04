using System;

namespace cCoder.Core.Objects
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DontPrivilegeAttribute : Attribute { }
}
