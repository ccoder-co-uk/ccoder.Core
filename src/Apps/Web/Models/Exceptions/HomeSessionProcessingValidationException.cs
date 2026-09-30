// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace Web.Models.Exceptions;

internal sealed class HomeSessionProcessingValidationException(
    Exception innerException)
    : Exception(
        message: "Home session processing validation failed.",
        innerException: innerException);