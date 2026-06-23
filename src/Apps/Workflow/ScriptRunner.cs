using System.Reflection;
using cCoder.Workflow.Activities;
using cCoder.Workflow.Activities.Models;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Workflow;

public sealed class ScriptRunner : IScriptRunner
{
    private readonly Assembly[] references;

    public ScriptRunner(LogEvent log)
    {
        try
        {
            List<Assembly> loadedAssemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .ToList();

            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string binDirectory = currentAssembly.Location.Replace(currentAssembly.ManifestModule.Name, string.Empty, StringComparison.Ordinal);

            string[] assembliesToLoad = Directory.GetFiles(binDirectory, "*.dll")
                .Where(path => loadedAssemblies.All(assembly =>
                    !string.Equals(assembly.Location, path, StringComparison.OrdinalIgnoreCase)))
                .Where(path => !path.Contains("api-ms-win", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (string assemblyPath in assembliesToLoad)
                SafelyLoadAssembly(log, loadedAssemblies, assemblyPath);

            references = loadedAssemblies.ToArray();
        }
        catch (Exception exception)
        {
            _ = log(WorkflowLogLevel.Warning, $"Script runner may be missing references but will continue: {exception.Message}");
            references = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .ToArray();
        }
    }

    public async Task<T> BuildScript<T>(string code, string[] imports, Action<WorkflowLogLevel, string> log)
    {
        try
        {
            ScriptOptions options = BuildOptions(imports);
            return await CSharpScript.EvaluateAsync<T>(code, options);
        }
        catch (Exception exception)
        {
            log(WorkflowLogLevel.Error, "Script failed to compile.");
            log(WorkflowLogLevel.Error, exception.Message);

            if (exception is Microsoft.CodeAnalysis.Scripting.CompilationErrorException compilationError)
                log(WorkflowLogLevel.Error, $"Source of the problem:{Environment.NewLine}{compilationError.Source}");

            return default;
        }
    }

    public async Task<T> Run<T>(
        string code,
        string[] imports,
        object args = null,
        Action<WorkflowLogLevel, string> log = null)
    {
        try
        {
            IEnumerable<Assembly> requiredReferences = ResolveReferences(imports);
            ScriptOptions options = ScriptOptions.Default
                .AddReferences(requiredReferences)
                .WithImports(imports);

            if (log is not null)
            {
                string details =
                    $"{Environment.NewLine}Imports{Environment.NewLine}  {string.Join($"{Environment.NewLine}  ", imports)}"
                    + $"{Environment.NewLine}{Environment.NewLine}References Needed{Environment.NewLine}  {string.Join($"{Environment.NewLine}  ", requiredReferences.Select(reference => reference.FullName))}";
                log(WorkflowLogLevel.Debug, details);
            }

            return (T)await CSharpScript.EvaluateAsync(code, options, args, args?.GetType());
        }
        catch (NullReferenceException exception)
        {
            string target = exception.TargetSite is null
                ? "unknown"
                : $"(({exception.TargetSite.DeclaringType?.Name ?? "object"})object).{exception.TargetSite.Name}";

            List<string> context = [];

            foreach (object key in exception.Data.Keys)
                context.Add($"{key}: {exception.Data[key]}");

            log?.Invoke(
                WorkflowLogLevel.Error,
                $"{exception.Message}{Environment.NewLine}Context: {exception.Source}{Environment.NewLine}Target: {target}{Environment.NewLine}{string.Join(Environment.NewLine, context)}");

            throw;
        }
        catch (Microsoft.CodeAnalysis.Scripting.CompilationErrorException exception)
        {
            log?.Invoke(
                WorkflowLogLevel.Error,
                $"Compilation failed:{Environment.NewLine}{exception.Message}{Environment.NewLine}{string.Join(Environment.NewLine, exception.Diagnostics)}");

            throw;
        }
    }

    public Task Run(string code, string[] imports, object args, Action<WorkflowLogLevel, string> log) =>
        Run<bool>($"{code};return true;", imports, args, log);

    private ScriptOptions BuildOptions(string[] imports) =>
        ScriptOptions.Default
            .AddReferences(ResolveReferences(imports))
            .WithImports(imports);

    private IEnumerable<Assembly> ResolveReferences(string[] imports) =>
        references.Where(reference =>
        {
            try
            {
                return reference.GetExportedTypes().Any(type => imports.Contains(type.Namespace));
            }
            catch
            {
                return false;
            }
        });

    private static void SafelyLoadAssembly(LogEvent log, ICollection<Assembly> loadedAssemblies, string assemblyPath)
    {
        try
        {
            Assembly assembly = Assembly.LoadFile(assemblyPath);
            loadedAssemblies.Add(assembly);
            _ = log(WorkflowLogLevel.Debug, $"Loaded assembly: {assembly.FullName}");
        }
        catch (Exception exception)
        {
            _ = log(WorkflowLogLevel.Warning, $"Unable to load assembly {assemblyPath}: {exception.Message}");
        }
    }
}
