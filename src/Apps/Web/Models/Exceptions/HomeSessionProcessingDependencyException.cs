// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace Web.Models.Exceptions;

internal sealed class HomeSessionProcessingDependencyException(
    Exception innerException)
    : Exception(
        message: "A Home session processing dependency failed.",
        innerException: innerException);