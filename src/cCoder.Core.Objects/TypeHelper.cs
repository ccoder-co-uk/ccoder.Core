using System.Reflection;

namespace cCoder.Core;

public static partial class TypeHelper
{
    private static Assembly[] stackAssemblies = null;

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
                .ToArray();
        }

        return stackAssemblies;
    }
}