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
using System.ComponentModel.DataAnnotations;

namespace Workflow.Services.Foundations.WorkflowFunctions;

internal sealed partial class WorkflowFunctionsService
{
    private static void ValidateInputs(params object[] inputs)
    {
        if (inputs.FirstOrDefault() is null)
        {
            throw new ValidationException(
                message: "A workflow function input is required.");
        }

        if (inputs.FirstOrDefault() is string value
            && string.IsNullOrWhiteSpace(value: value))
        {
            throw new ValidationException(
                message: "A workflow function value is required.");
        }
    }
}