// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models.Exceptions;

internal sealed class ApiCacheServiceException(
    Exception innerException)
    : Exception(
        message: "The API cache service failed.",
        innerException: innerException);