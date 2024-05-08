using System.Reflection;

namespace cCoder.Core;

public static partial class TypeHelper
{
    private class AssemblyComparer : IEqualityComparer<Assembly>
    {
        public bool Equals(Assembly x, Assembly y) => x.FullName == y.FullName;

        public int GetHashCode(Assembly obj) => base.GetHashCode();
    }
}