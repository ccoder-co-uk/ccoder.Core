namespace cCoder.Core.Objects.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class DontPrivilegeAttribute : Attribute { }
