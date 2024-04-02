using Core.Objects;
using Core.Objects.Dtos.Workflow;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;

namespace Workflow.Framework
{
    public class ScriptRunner : IScriptRunner
    {
        readonly Assembly[] references;

        public ScriptRunner(LogEvent log)
        {
            try
            {
                // some of the stack might not have been loaded, lets make sure we load everything so we can give a complete response
                // first grab what's loaded
                List<Assembly> loadedAlready = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .Where(a => !a.IsDynamic)
                        .ToList();

                // then grab the bin directory
                Assembly thisAssembly = Assembly.GetExecutingAssembly();
                string binDir = thisAssembly.Location
                    .Replace(thisAssembly.ManifestModule.Name, "");

                // from the bin, grab our core dll files
                List<string> stackDlls = Directory.GetFiles(binDir)
                    .Where(f => f.EndsWith("dll"))
                    .ToList();

                // load the missing ones
                string[] toLoad = stackDlls
                    .Where(assemblyPath => loadedAlready.All(a => a.Location.ToLowerInvariant() != assemblyPath.ToLowerInvariant()))
                    .Where(a => !a.Contains("api-ms-win"))
                    .ToArray();

                foreach (string assemblyPath in toLoad)
                    SafelyLoadAssembly(log, loadedAlready, assemblyPath);

                references = loadedAlready.ToArray();
            }
            catch (Exception ex)
            {
                _ = log(WorkflowLogLevel.Warning, "Script Runner may not have everything it needs, continuing anyway despite exception:\n" + ex.Message);
            }
        }

        static void SafelyLoadAssembly(LogEvent log, List<Assembly> loadedAlready, string assemblyPath)
        {
            try
            {
                Assembly a = Assembly.LoadFile(assemblyPath);
                loadedAlready.Add(a);
                _ = log(WorkflowLogLevel.Info, $"Loaded: {a.FullName} ");
            }
            catch (Exception ex)
            {
                _ = log(WorkflowLogLevel.Warning, $"Unable to load assembly {assemblyPath} because: " + ex.Message);
            }
        }

        public async Task<T> BuildScript<T>(string code, string[] imports, Action<WorkflowLogLevel, string> log)
        {
            try
            {
                IEnumerable<Assembly> referencesNeeded = references.Where(r =>
                {
                    try
                    {
                        return r.GetExportedTypes().Any(t => imports.Contains(t.Namespace));
                    }
                    catch { return false; }
                });
                ScriptOptions options = ScriptOptions.Default
                    .AddReferences(referencesNeeded)
                    .WithImports(imports);

                T result = await CSharpScript.EvaluateAsync<T>(code, options);
                return result;
            }
            catch (Exception ex)
            {
                log(WorkflowLogLevel.Error, "Script failed to compile.");
                log(WorkflowLogLevel.Error, ex.Message);

                if (ex is CompilationErrorException cEx)
                    log(WorkflowLogLevel.Error, $"Source of the problem:\n{cEx.Source}");

                return default;
            }
        }

        public async Task<T> Run<T>(string code, string[] imports, object args = null, Action<WorkflowLogLevel, string> log = null)
        {
            try
            {
                IEnumerable<Assembly> referencesNeeded = references.Where(r =>
                {
                    try
                    {
                        return r.GetExportedTypes().Any(t => imports.Contains(t.Namespace));
                    }
                    catch { return false; }
                });
                ScriptOptions options = ScriptOptions.Default
                    .AddReferences(referencesNeeded)
                    .WithImports(imports);

                if (log != null)
                {
                    string message = $"\nImports\n  {string.Join("\n  ", imports)}\n\nReferences Needed\n  {string.Join("\n  ", referencesNeeded.Select(r => r.FullName))}";
                    log(WorkflowLogLevel.Debug, message);
                }

                return (T)await CSharpScript.EvaluateAsync(code, options, args, args?.GetType());
            }
            catch (NullReferenceException ex)
            {
                string typeAndCall = $"(({ex.TargetSite.DeclaringType.Name})object).{ex.TargetSite.Name}";
                List<string> data = new();

                foreach (object k in ex.Data.Keys)
                    data.Add($"{k}: {ex.Data[k]}");

                log?.Invoke(WorkflowLogLevel.Error, ex.Message + $"\nContext: {ex.Source}\nTarget: {typeAndCall}\n{string.Join("\n", data)}");

                throw;
            }
            catch (CompilationErrorException ex)
            {
                log?.Invoke(WorkflowLogLevel.Error, $"Compilation failed:\n{ex.Message}\n{string.Join(Environment.NewLine, ex.Diagnostics)}");

                throw;
            }
        }

        public Task Run(string code, string[] imports, object args, Action<WorkflowLogLevel, string> log)
            => Run<bool>(code + ";return true;", imports, args, log);
    }
}