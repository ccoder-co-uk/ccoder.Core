// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreProcessingValidationException(Exception innerException)
    : Exception("Core processing validation failed.", innerException);