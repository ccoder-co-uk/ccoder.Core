using Core.Objects;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Core
{
    public static partial class TypeHelper
    {
        static Assembly[] stackAssemblies = null;

        public static string[] StackPrefixes = new[] { "Core", "B2B", "Security" };

        /// <summary>
        /// Gets the array of core stack assemblies to use for type searching.
        /// </summary>
        /// <returns></returns>
        public static Assembly[] GetWebStackAssemblies()
        {
            if (stackAssemblies == null)
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                var loadedPaths = loadedAssemblies
                    .Where(a => !a.IsDynamic)
                    .Select(a => a.Location)
                    .ToArray();

                var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
                var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();

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
}