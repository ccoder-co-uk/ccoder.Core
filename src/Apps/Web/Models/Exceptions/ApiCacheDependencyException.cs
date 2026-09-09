// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models.Exceptions;

internal sealed class ApiCacheDependencyException(
    Exception innerException)
    : Exception(
        message: "An API cache dependency failed.",
        innerException: innerException);