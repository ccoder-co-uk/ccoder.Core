// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Workflow.Engine.Models.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Workflow.Services.Foundations.WorkflowFunctions;

internal sealed partial class WorkflowFunctionsService
{
    private static async ValueTask<TResult> TryCatch<TResult>(
        Func<ValueTask<TResult>> operation)
    {
        try
        {
            return await operation();
        }
        catch (ValidationException innerException)
        {
            throw new WorkflowEngineValidationException(
                innerException: innerException);
        }
        catch (WorkflowEngineDependencyException innerException)
        {
            throw new WorkflowEngineDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new WorkflowEngineServiceException(
                innerException: innerException);
        }
    }

    private static TResult TryCatch<TResult>(Func<TResult> operation)
    {
        try
        {
            return operation();
        }
        catch (ValidationException innerException)
        {
            throw new WorkflowEngineValidationException(
                innerException: innerException);
        }
        catch (WorkflowEngineDependencyException innerException)
        {
            throw new WorkflowEngineDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new WorkflowEngineServiceException(
                innerException: innerException);
        }
    }

    private static async Task<TResult> TryCatch<TResult>(
        Func<Task<TResult>> operation)
    {
        try
        {
            return await operation();
        }
        catch (ValidationException innerException)
        {
            throw new WorkflowEngineValidationException(
                innerException: innerException);
        }
        catch (WorkflowEngineDependencyException innerException)
        {
            throw new WorkflowEngineDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new WorkflowEngineServiceException(
                innerException: innerException);
        }
    }

    private static async Task TryCatch(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (ValidationException innerException)
        {
            throw new WorkflowEngineValidationException(
                innerException: innerException);
        }
        catch (WorkflowEngineDependencyException innerException)
        {
            throw new WorkflowEngineDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new WorkflowEngineServiceException(
                innerException: innerException);
        }
    }

    private static void TryCatch(Action operation)
    {
        try
        {
            operation();
        }
        catch (ValidationException innerException)
        {
            throw new WorkflowEngineValidationException(
                innerException: innerException);
        }
        catch (WorkflowEngineDependencyException innerException)
        {
            throw new WorkflowEngineDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new WorkflowEngineServiceException(
                innerException: innerException);
        }
    }
}