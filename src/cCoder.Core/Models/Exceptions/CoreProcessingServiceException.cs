// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreProcessingServiceException(Exception innerException)
    : Exception("The Core processing service failed.", innerException);