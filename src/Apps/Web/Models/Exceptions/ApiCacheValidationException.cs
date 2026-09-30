// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace Web.Models.Exceptions;

internal sealed class ApiCacheValidationException(
    Exception innerException)
    : Exception(
        message: "API cache validation failed.",
        innerException: innerException);