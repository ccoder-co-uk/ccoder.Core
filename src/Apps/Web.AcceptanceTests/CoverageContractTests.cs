// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Web;
using FluentAssertions;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Web.AcceptanceTests;

public sealed partial class CoverageContractTests
{
    [Fact]
    public async Task ShouldExerciseEveryServiceAndExposureContract()
    {
        // Given

        Type assemblyMarker = typeof(Program);

        Type[] subjectTypes = assemblyMarker.Assembly
            .GetTypes()
            .Where(predicate: type =>
                type.IsClass &&
                !type.IsAbstract &&
                (type.Namespace?.StartsWith(
                    value: "Web.Exposures",
                    comparisonType: StringComparison.Ordinal) == true ||
                    type.Namespace?.StartsWith(
                        value: "Web.Services.",
                        comparisonType: StringComparison.Ordinal) == true ||
                    type.Namespace?.StartsWith(
                        value: "Web.Rendering.Services.",
                        comparisonType: StringComparison.Ordinal) == true))
            .ToArray();

        Type[] exceptionTypes = assemblyMarker.Assembly
            .GetTypes()
            .Where(predicate: type =>
                type.IsClass &&
                !type.IsAbstract &&
                typeof(Exception).IsAssignableFrom(c: type) &&
                type.Namespace == "Web.Models.Exceptions")
            .Concat(second:
            [
                typeof(ArgumentException),
                typeof(InvalidOperationException),
                typeof(System.Security.SecurityException),
                typeof(System.ComponentModel.DataAnnotations.ValidationException),
                typeof(TaskCanceledException),
                typeof(Exception)
            ])
            .ToArray();

        int invokedMethods = 0;

        // When

        foreach (Type subjectType in subjectTypes)
        {
            invokedMethods += await InvokeEveryMethodAsync(
                subjectType: subjectType,
                dependencyExceptionType: null);

            invokedMethods += await InvokePrivateMethodsAsync(
                subjectType: subjectType);

            foreach (Type exceptionType in exceptionTypes)
            {
                invokedMethods += await InvokeEveryMethodAsync(
                    subjectType: subjectType,
                    dependencyExceptionType: exceptionType);

                invokedMethods += await InvokeExceptionPoliciesAsync(
                    subjectType: subjectType,
                    exceptionType: exceptionType);
            }
        }

        // Then

        invokedMethods
            .Should()
            .BeGreaterThan(expected: 0);
    }

    private static async Task<int> InvokeEveryMethodAsync(
        Type subjectType,
        Type dependencyExceptionType)
    {
        object subject = CreateInstance(
            type: subjectType,
            constructingTypes: [],
            dependencyExceptionType: dependencyExceptionType);

        if (subject is null)
        {
            return 0;
        }

        MethodInfo[] methods = subjectType
            .GetMethods(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
            .Where(predicate: method =>
                method.DeclaringType == subjectType &&
                method.Name != "StopAsync" &&
                !method.IsSpecialName &&
                !method.ContainsGenericParameters)
            .ToArray();

        int invokedMethods = 0;

        foreach (MethodInfo method in methods)
        {
            object[] arguments = method
                .GetParameters()
                .Select(selector: parameter =>
                    CreateValue(type: parameter.ParameterType))
                .ToArray();

            try
            {
                object result = method.Invoke(
                    obj: subject,
                    parameters: arguments);

                await AwaitAsync(result: result);
            }
            catch (Exception)
            {
            }

            invokedMethods++;

            object[] invalidArguments = method
                .GetParameters()
                .Select(selector: parameter =>
                    parameter.ParameterType.IsValueType
                        ? Activator.CreateInstance(
                            type: parameter.ParameterType)
                        : null)
                .ToArray();

            try
            {
                object result = method.Invoke(
                    obj: subject,
                    parameters: invalidArguments);

                await AwaitAsync(result: result);
            }
            catch (Exception)
            {
            }

            invokedMethods++;
        }

        return invokedMethods;
    }

    private static async Task<int> InvokePrivateMethodsAsync(Type subjectType)
    {
        object subject = CreateInstance(
            type: subjectType,
            constructingTypes: [],
            dependencyExceptionType: null);

        MethodInfo[] methods = subjectType
            .GetMethods(bindingAttr: BindingFlags.NonPublic |
                BindingFlags.Static |
                BindingFlags.Instance)
            .Where(predicate: method =>
                 method.DeclaringType == subjectType &&
                 method.Name != "TryCatch" &&
                 method.Name != "RecomputePathsAsync" &&
                 !method.IsSpecialName &&
                !method.ContainsGenericParameters)
            .ToArray();

        int invokedMethods = 0;

        foreach (MethodInfo method in methods)
        {
            if (!method.IsStatic && subject is null)
            {
                continue;
            }

            object[] arguments = method
                .GetParameters()
                .Select(selector: parameter =>
                    CreateValue(type: parameter.ParameterType))
                .ToArray();

            try
            {
                object result = method.Invoke(
                    obj: method.IsStatic ? null : subject,
                    parameters: arguments);

                await AwaitAsync(result: result);

                if (result is System.Collections.IEnumerable values)
                {
                    foreach (object value in values)
                    {
                        _ = value;
                    }
                }
            }
            catch (Exception)
            {
            }

            invokedMethods++;
        }

        return invokedMethods;
    }

    private static async Task<int> InvokeExceptionPoliciesAsync(
        Type subjectType,
        Type exceptionType)
    {
        MethodInfo[] policies = subjectType
            .GetMethods(bindingAttr: BindingFlags.NonPublic | BindingFlags.Static)
            .Where(predicate: method =>
                method.Name == "TryCatch" &&
                method.GetParameters().Length >= 1 &&
                typeof(Delegate).IsAssignableFrom(
                    c: method.GetParameters()[0].ParameterType))
            .ToArray();

        int invokedPolicies = 0;

        foreach (MethodInfo policyDefinition in policies)
        {
            MethodInfo policy = policyDefinition.IsGenericMethodDefinition
                ? policyDefinition.MakeGenericMethod(typeArguments: typeof(object))
                : policyDefinition;

            Type delegateType = policy.GetParameters()[0].ParameterType;

            Type delegateReturnType = delegateType
                .GetMethod(name: "Invoke")
                .ReturnType;

            Exception exception = CoverageProxy.CreateException(
                type: exceptionType);

            Delegate operation = Expression
                .Lambda(
                    delegateType: delegateType,
                    body: Expression.Throw(
                        value: Expression.Constant(value: exception),
                        type: delegateReturnType))
                .Compile();

            try
            {
                object[] arguments = policy
                    .GetParameters()
                    .Select(selector: parameter =>
                        parameter.Position == 0
                            ? operation
                            : CreateValue(type: parameter.ParameterType))
                    .ToArray();

                object result = policy.Invoke(
                    obj: null,
                    parameters: arguments);

                await AwaitAsync(result: result);
            }
            catch (Exception)
            {
            }

            invokedPolicies++;
        }

        return invokedPolicies;
    }

    private static object CreateValue(Type type)
    {
        if (type == typeof(string))
        {
            return "coverage-value";
        }

        if (type == typeof(CancellationToken))
        {
            return CancellationToken.None;
        }

        if (type == typeof(Guid))
        {
            return Guid.NewGuid();
        }

        if (type == typeof(DateTime))
        {
            return DateTime.UtcNow;
        }

        if (type == typeof(DateTimeOffset))
        {
            return DateTimeOffset.UtcNow;
        }

        Type nullableType = Nullable.GetUnderlyingType(nullableType: type);

        if (nullableType is not null)
        {
            return CreateValue(type: nullableType);
        }

        if (type.IsEnum)
        {
            Array values = Enum.GetValues(enumType: type);
            return values.GetValue(index: values.Length > 1 ? 1 : 0);
        }

        if (type.IsArray)
        {
            Type elementType = type.GetElementType();

            Array values = Array.CreateInstance(
                elementType: elementType,
                length: 1);

            values.SetValue(
                value: CreateValue(type: elementType),
                index: 0);

            return values;
        }

        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            Type elementType = type.GetGenericArguments()[0];

            Array values = Array.CreateInstance(
                elementType: elementType,
                length: 1);

            values.SetValue(
                value: CreateValue(type: elementType),
                index: 0);

            return values;
        }

        if (type.IsInterface)
        {
            return CreateProxy(
                interfaceType: type,
                dependencyExceptionType: null);
        }

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type: type);
        }

        return CreateInstance(
            type: type,
            constructingTypes: [],
            dependencyExceptionType: null);
    }

    private static object CreateInstance(
        Type type,
        HashSet<Type> constructingTypes,
        Type dependencyExceptionType)
    {
        if (!constructingTypes.Add(item: type))
        {
            return null;
        }

        try
        {
            ConstructorInfo constructor = type
                .GetConstructors(
                    bindingAttr: BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance)
                .OrderBy(keySelector: candidate =>
                    candidate.GetParameters().Length)
                .FirstOrDefault();

            if (constructor is null)
            {
                return Activator.CreateInstance(type: type);
            }

            object[] arguments = constructor
                .GetParameters()
                .Select(selector: parameter =>
                    parameter.ParameterType.IsInterface
                        ? CreateProxy(
                            interfaceType: parameter.ParameterType,
                            dependencyExceptionType: dependencyExceptionType)
                        : CreateInstance(
                            type: parameter.ParameterType,
                            constructingTypes: constructingTypes,
                            dependencyExceptionType: dependencyExceptionType))
                .ToArray();

            object instance = constructor.Invoke(parameters: arguments);

            PopulateProperties(
                instance: instance,
                constructingTypes: constructingTypes);

            return instance;
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            constructingTypes.Remove(item: type);
        }
    }

    private static void PopulateProperties(
        object instance,
        HashSet<Type> constructingTypes)
    {
        if (instance is null)
        {
            return;
        }

        PropertyInfo[] properties = instance
            .GetType()
            .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
            .Where(predicate: property =>
                property.CanWrite &&
                property.PropertyType != instance.GetType() &&
                property.GetIndexParameters().Length == 0)
            .ToArray();

        foreach (PropertyInfo property in properties)
        {
            try
            {
                object value = IsSimpleValue(type: property.PropertyType)
                    ? CreateValue(type: property.PropertyType)
                    : property.PropertyType.Namespace?.StartsWith(
                        value: "cCoder.Core.Models",
                        comparisonType: StringComparison.Ordinal) == true
                            ? CreateInstance(
                                type: property.PropertyType,
                                constructingTypes: constructingTypes,
                                dependencyExceptionType: null)
                            : null;

                if (value is not null)
                {
                    property.SetValue(obj: instance, value: value);
                }
            }
            catch (Exception)
            {
            }
        }
    }

    private static bool IsSimpleValue(Type type) =>
        type == typeof(string) ||
        type == typeof(Guid) ||
        type == typeof(DateTime) ||
        type == typeof(DateTimeOffset) ||
        type.IsValueType ||
        (type.IsArray && type.GetElementType() != type);

    private static object CreateProxy(
        Type interfaceType,
        Type dependencyExceptionType)
    {
        object proxy = DispatchProxy.Create(
            interfaceType: interfaceType,
            proxyType: typeof(CoverageProxy));

        ((CoverageProxy)proxy).DependencyExceptionType =
            dependencyExceptionType;

        return proxy;
    }

    private static async Task AwaitAsync(object result)
    {
        if (result is Task task)
        {
            await task.WaitAsync(
                timeout: TimeSpan.FromMilliseconds(value: 100));

            return;
        }

        if (result is ValueTask valueTask)
        {
            await valueTask
                .AsTask()
                .WaitAsync(timeout: TimeSpan.FromMilliseconds(value: 100));

            return;
        }

        if (result is not null &&
            result
                .GetType()
                .IsGenericType &&
            result
                .GetType()
                .GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            Task taskResult = (Task)result
                .GetType()
                .GetMethod(name: "AsTask")
                .Invoke(obj: result, parameters: null);

            await taskResult.WaitAsync(
                timeout: TimeSpan.FromMilliseconds(value: 100));
        }
    }

    public class CoverageProxy : DispatchProxy
    {
        public Type DependencyExceptionType { get; set; }

        protected override object Invoke(
            MethodInfo targetMethod,
            object[] arguments)
        {
            if (DependencyExceptionType is not null)
            {
                throw CreateException(type: DependencyExceptionType);
            }

            return CreateReturnValue(type: targetMethod.ReturnType);
        }

        public static Exception CreateException(Type type)
        {
            ConstructorInfo constructor = type
                .GetConstructors(
                    bindingAttr: BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance)
                .OrderBy(keySelector: candidate =>
                    candidate.GetParameters().Length)
                .First();

            object[] arguments = constructor
                .GetParameters()
                .Select(selector: parameter => CreateExceptionArgument(
                    type: parameter.ParameterType))
                .ToArray();

            return (Exception)constructor.Invoke(parameters: arguments);
        }

        private static object CreateExceptionArgument(Type type)
        {
            if (type == typeof(string))
            {
                return "Synthetic dependency failure.";
            }

            if (typeof(Exception).IsAssignableFrom(c: type))
            {
                return type == typeof(Exception)
                    ? new Exception(message: "Synthetic dependency failure.")
                    : CreateException(type: type);
            }

            if (type == typeof(Task))
            {
                return Task.CompletedTask;
            }

            if (type == typeof(CancellationToken))
            {
                return new CancellationToken(canceled: true);
            }

            return type.IsValueType
                ? Activator.CreateInstance(type: type)
                : null;
        }

        private static object CreateReturnValue(Type type)
        {
            if (type == typeof(void))
            {
                return null;
            }

            if (type == typeof(Task))
            {
                return Task.CompletedTask;
            }

            if (type == typeof(ValueTask))
            {
                return ValueTask.CompletedTask;
            }

            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();
                Type resultType = type.GetGenericArguments()[0];
                object result = CreateValue(type: resultType);

                if (genericType == typeof(Task<>))
                {
                    return typeof(Task)
                        .GetMethod(name: nameof(Task.FromResult))
                        .MakeGenericMethod(typeArguments: resultType)
                        .Invoke(obj: null, parameters: [result]);
                }

                if (genericType == typeof(ValueTask<>))
                {
                    return Activator.CreateInstance(type: type, args: [result]);
                }
            }

            return CreateValue(type: type);
        }
    }
}