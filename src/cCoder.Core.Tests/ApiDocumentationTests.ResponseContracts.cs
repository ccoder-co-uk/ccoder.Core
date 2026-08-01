// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies.OpenApi;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OpenApi;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace cCoder.Core.Tests;

public sealed partial class HttpResponseContractOperationFilterTests
{
    [Fact]
    public void ShouldDocumentODataCreateResponseContract()
    {
        // Given
        OpenApiOperation operation = CreateOperation();

        ApiDescription apiDescription = CreateApiDescription(
            httpMethod: "POST",
            relativePath: "Api/Tests/Things",
            hasRequestBody: true,
            authorize: true);

        OperationFilterContext context = CreateContext(
            apiDescription: apiDescription,
            methodName: nameof(ResponseContractTestController.Post));

        HttpResponseContractOperationFilter filter = new();

        // When
        filter.Apply(operation: operation, context: context);

        // Then
        operation.Responses.Keys.Should()
            .BeEquivalentTo(expectation: ["201", "400", "401", "403", "415", "500"]);
    }

    [Fact]
    public void ShouldDocumentODataDeleteResponseContract()
    {
        // Given
        OpenApiOperation operation = CreateOperation();

        ApiDescription apiDescription = CreateApiDescription(
            httpMethod: "DELETE",
            relativePath: "Api/Tests/Things({key})");

        OperationFilterContext context = CreateContext(
            apiDescription: apiDescription,
            methodName: nameof(ResponseContractTestController.Delete));

        HttpResponseContractOperationFilter filter = new();

        // When
        filter.Apply(operation: operation, context: context);

        // Then
        operation.Responses.Keys.Should()
            .BeEquivalentTo(expectation: ["204", "404", "500"]);
    }

    [Fact]
    public void ShouldPreserveBoundActionSuccessResponseContract()
    {
        // Given
        OpenApiOperation operation = CreateOperation();

        ApiDescription apiDescription = CreateApiDescription(
            httpMethod: "POST",
            relativePath: "Api/Tests/Things({key})/Publish");

        OperationFilterContext context = CreateContext(
            apiDescription: apiDescription,
            methodName: nameof(ResponseContractTestController.Publish));

        HttpResponseContractOperationFilter filter = new();

        // When
        filter.Apply(operation: operation, context: context);

        // Then
        operation.Responses.Keys.Should()
            .BeEquivalentTo(expectation: ["200", "404", "500"]);
    }

    [Fact]
    public void ShouldDocumentCaughtExceptionResponseContract()
    {
        // Given
        OpenApiOperation operation = CreateOperation();

        ApiDescription apiDescription = CreateApiDescription(
            httpMethod: "PUT",
            relativePath: "Api/Tests/Things({key})",
            hasRequestBody: true);

        OperationFilterContext context = CreateContext(
            apiDescription: apiDescription,
            methodName: nameof(ResponseContractTestController.Put));

        HttpResponseContractOperationFilter filter = new();

        // When
        filter.Apply(operation: operation, context: context);

        // Then
        operation.Responses.Keys.Should()
            .BeEquivalentTo(expectation: ["200", "400", "403", "404", "409", "415", "500"]);
    }

    private static ApiDescription CreateApiDescription(
        string httpMethod,
        string relativePath,
        bool hasRequestBody = false,
        bool authorize = false)
    {
        ApiDescription apiDescription = new()
        {
            ActionDescriptor = new ActionDescriptor
            {
                EndpointMetadata = new List<object>(),
            },
            HttpMethod = httpMethod,
            RelativePath = relativePath,
        };

        if (hasRequestBody)
        {
            apiDescription.ParameterDescriptions.Add(item: new()
            {
                Source = BindingSource.Body,
            });
        }

        if (authorize)
        {
            apiDescription.ActionDescriptor.EndpointMetadata.Add(item: new AuthorizeAttribute());
        }

        return apiDescription;
    }

    private static OperationFilterContext CreateContext(
        ApiDescription apiDescription,
        string methodName) =>
        new(
            apiDescription,
            Mock.Of<ISchemaGenerator>(),
            new SchemaRepository(),
            new OpenApiDocument(),
            typeof(ResponseContractTestController).GetMethod(name: methodName));

    private static OpenApiOperation CreateOperation() =>
        new()
        {
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "OK",
                },
            },
        };

    private sealed class ResponseContractTestController : ODataController
    {
        public IActionResult Delete(int key) =>
            NoContent();

        public IActionResult Post(object value) =>
            StatusCode(statusCode: 201, value: value);

        public IActionResult Publish(int key) =>
            Ok(value: key);

        public IActionResult Put(object value)
        {
            try
            {
                return Ok(value: value);
            }
            catch (TestValidationException)
            {
                return BadRequest();
            }
            catch (TestAuthorizationException)
            {
                return StatusCode(statusCode: 403);
            }
            catch (TestConcurrencyException)
            {
                return Conflict();
            }
            catch (Exception)
            {
                return StatusCode(statusCode: 500);
            }
        }
    }

    private sealed class TestAuthorizationException : Exception;

    private sealed class TestConcurrencyException : Exception;

    private sealed class TestValidationException : Exception;
}