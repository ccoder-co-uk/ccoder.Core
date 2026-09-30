// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using cCoder.Workflow.Engine.Models.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Workflow.Services.Processings.WorkflowFunctions;

internal sealed partial class WorkflowFunctionsProcessingService
{
    private static async Task<TResult> TryCatch<TResult>(
        Func<Task<TResult>> operation)
    {
        try
        {
            return await operation();
        }
        catch (WorkflowEngineValidationException innerException)
        {
            throw new WorkflowEngineValidationException(
                innerException: innerException);
        }
        catch (WorkflowEngineDependencyException innerException)
        {
            throw new WorkflowEngineDependencyException(
                innerException: innerException);
        }
        catch (ValidationException innerException)
        {
            throw new WorkflowEngineValidationException(
                innerException: innerException);
        }
        catch (InvalidOperationException innerException)
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
        catch (WorkflowEngineValidationException innerException)
        {
            throw new WorkflowEngineValidationException(
                innerException: innerException);
        }
        catch (WorkflowEngineDependencyException innerException)
        {
            throw new WorkflowEngineDependencyException(
                innerException: innerException);
        }
        catch (ValidationException innerException)
        {
            throw new WorkflowEngineValidationException(
                innerException: innerException);
        }
        catch (InvalidOperationException innerException)
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