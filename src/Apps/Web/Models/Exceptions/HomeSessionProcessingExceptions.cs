// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models.Exceptions;

internal sealed class HomeSessionProcessingValidationException(
    Exception innerException)
    : Exception(
        message: "Home session processing validation failed.",
        innerException: innerException);

internal sealed class HomeSessionProcessingServiceException(
    Exception innerException)
    : Exception(
        message: "Home session processing failed.",
        innerException: innerException);

internal sealed class HomeSessionProcessingDependencyException(
    Exception innerException)
    : Exception(
        message: "A Home session processing dependency failed.",
        innerException: innerException);