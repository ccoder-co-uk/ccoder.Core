// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace cCoder.Core.Dependencies.OpenApi;

internal sealed class HttpResponseContractOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(argument: operation);
        ArgumentNullException.ThrowIfNull(argument: context);

        ApiDescription apiDescription = context.ApiDescription;
        string httpMethod = apiDescription.HttpMethod?.ToUpperInvariant() ?? string.Empty;
        string relativePath = apiDescription.RelativePath ?? string.Empty;
        bool hasRequestBody = apiDescription.ParameterDescriptions.Any(parameter =>
            parameter.Source?.Id == "Body");
        bool hasKey = relativePath.Contains("{key}", StringComparison.OrdinalIgnoreCase)
            || relativePath.Contains("{id}", StringComparison.OrdinalIgnoreCase);
        bool isODataBoundOperation = relativePath.Contains(")/", StringComparison.Ordinal);
        bool isODataController = InheritsFromODataController(type: context.MethodInfo.DeclaringType);
        bool requiresAuthorization = RequiresAuthorization(apiDescription: apiDescription);

        NormalizeSuccessResponse(
            operation: operation,
            httpMethod: httpMethod,
            isODataController: isODataController,
            isODataBoundOperation: isODataBoundOperation,
            methodName: context.MethodInfo.Name);

        AddCaughtExceptionResponses(
            operation: operation,
            methodInfo: context.MethodInfo);

        AddResponse(operation: operation, statusCode: 500, description: "Internal Server Error");

        if (requiresAuthorization)
        {
            AddResponse(operation: operation, statusCode: 401, description: "Unauthorized");
            AddResponse(operation: operation, statusCode: 403, description: "Forbidden");
        }

        if (hasRequestBody)
        {
            AddResponse(operation: operation, statusCode: 400, description: "Bad Request");
            AddResponse(operation: operation, statusCode: 415, description: "Unsupported Media Type");
        }

        if (hasKey)
        {
            AddResponse(operation: operation, statusCode: 404, description: "Not Found");
        }

    }

    private static void AddCaughtExceptionResponses(
        OpenApiOperation operation,
        System.Reflection.MethodInfo methodInfo)
    {
        IEnumerable<string> caughtExceptionTypes = methodInfo
            .GetMethodBody()?
            .ExceptionHandlingClauses
            .Where(clause => clause.Flags == System.Reflection.ExceptionHandlingClauseOptions.Clause)
            .Select(clause => clause.CatchType?.Name ?? string.Empty)
            ?? [];

        foreach (string exceptionType in caughtExceptionTypes)
        {
            if (exceptionType.Contains("Authentication", StringComparison.Ordinal))
            {
                AddResponse(operation: operation, statusCode: 401, description: "Unauthorized");
            }
            else if (exceptionType.Contains("Authorization", StringComparison.Ordinal)
                || exceptionType.Contains("Security", StringComparison.Ordinal))
            {
                AddResponse(operation: operation, statusCode: 403, description: "Forbidden");
            }
            else if (exceptionType.Contains("Concurrency", StringComparison.Ordinal)
                || exceptionType.Contains("Conflict", StringComparison.Ordinal))
            {
                AddResponse(operation: operation, statusCode: 409, description: "Conflict");
            }
            else if (exceptionType.Contains("Precondition", StringComparison.Ordinal)
                || exceptionType.Contains("ETag", StringComparison.OrdinalIgnoreCase))
            {
                AddResponse(operation: operation, statusCode: 412, description: "Precondition Failed");
            }
            else if (exceptionType.Contains("UnsupportedMedia", StringComparison.Ordinal))
            {
                AddResponse(operation: operation, statusCode: 415, description: "Unsupported Media Type");
            }
            else if (exceptionType.Contains("Validation", StringComparison.Ordinal))
            {
                AddResponse(operation: operation, statusCode: 400, description: "Bad Request");
            }
        }
    }

    private static void AddResponse(
        OpenApiOperation operation,
        int statusCode,
        string description)
    {
        string responseKey = statusCode.ToString(provider: null);

        if (!operation.Responses.ContainsKey(key: responseKey))
        {
            operation.Responses.Add(
                key: responseKey,
                value: new OpenApiResponse
                {
                    Description = description,
                });
        }
    }

    private static void NormalizeSuccessResponse(
        OpenApiOperation operation,
        string httpMethod,
        bool isODataController,
        bool isODataBoundOperation,
        string methodName)
    {
        if (isODataController && httpMethod == "POST" && methodName == "Post")
        {
            operation.Responses.Remove(key: "200");
            AddResponse(operation: operation, statusCode: 201, description: "Created");
        }
        else if (isODataController
            && httpMethod == "DELETE"
            && !isODataBoundOperation
            && methodName == "Delete")
        {
            operation.Responses.Remove(key: "200");
            AddResponse(operation: operation, statusCode: 204, description: "No Content");
        }
    }

    private static bool RequiresAuthorization(ApiDescription apiDescription)
    {
        IList<object> metadata = apiDescription.ActionDescriptor.EndpointMetadata;

        if (metadata.OfType<IAllowAnonymous>().Any())
        {
            return false;
        }

        return metadata.OfType<IAuthorizeData>().Any();
    }

    private static bool InheritsFromODataController(Type type)
    {
        for (Type currentType = type; currentType is not null; currentType = currentType.BaseType)
        {
            if (currentType.Name == "ODataController")
            {
                return true;
            }
        }

        return false;
    }
}