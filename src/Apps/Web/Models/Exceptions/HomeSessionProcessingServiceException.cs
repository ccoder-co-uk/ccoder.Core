// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models.Exceptions;

internal sealed class HomeSessionProcessingServiceException(
    Exception innerException)
    : Exception(
        message: "Home session processing failed.",
        innerException: innerException);