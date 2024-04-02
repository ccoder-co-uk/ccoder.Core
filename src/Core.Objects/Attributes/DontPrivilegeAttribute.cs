using System;

namespace Core.Objects
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DontPrivilegeAttribute : Attribute { }
}
