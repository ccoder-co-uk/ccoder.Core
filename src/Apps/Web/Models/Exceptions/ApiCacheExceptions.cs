// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models.Exceptions;

internal sealed class ApiCacheValidationException(
    Exception innerException)
    : Exception(
        message: "API cache validation failed.",
        innerException: innerException);

internal sealed class ApiCacheDependencyException(
    Exception innerException)
    : Exception(
        message: "An API cache dependency failed.",
        innerException: innerException);

internal sealed class ApiCacheServiceException(
    Exception innerException)
    : Exception(
        message: "The API cache service failed.",
        innerException: innerException);