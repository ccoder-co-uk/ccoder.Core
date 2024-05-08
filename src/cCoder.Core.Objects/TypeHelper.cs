using cCoder.Core.Objects;
using System.Reflection;

namespace cCoder.Core;

public static partial class TypeHelper
{
    private static Assembly[] stackAssemblies = null;

    public static string[] StackPrefixes = new[] { "cCoder.Core", "B2B", "Security" };

    /// <summary>
    /// Gets the array of core stack assemblies to use for type searching.
    /// </summary>
    /// <returns></returns>
    public static Assembly[] GetWebStackAssemblies()
    {
        if (stackAssemblies == null)
        {
            List<Assembly> loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            string[] loadedPaths = loadedAssemblies
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location)
                .ToArray();

            string[] referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            List<string> toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();

            toLoad.ForEach(path =>
            {
                try
                {
                    loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path)));
                }
                catch { /* Probably some special framework / azure library */ }
            });

            stackAssemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => StackPrefixes.Any(p => a.FullName.StartsWith(p)))
                .ToArray();
        }

        return stackAssemblies;
    }

    /// <summary>
    /// Gets the array of data contexts that can be used for Api functionality.
    /// </summary>
    /// <returns></returns>
    public static Type[] GetContextTypes() => GetWebStackAssemblies()
        .SelectMany(a => a.GetTypes())
        .Where(t => t != typeof(IDataContext) && typeof(IDataContext).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
        .Distinct()
        .ToArray();

    public static Type[] GetEntityTypesFor(Type contextType) => contextType.GetProperties()
        .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetInterface("IQueryable") != null)
        .Select(p => p.PropertyType.GenericTypeArguments[0])
        .ToArray();
}