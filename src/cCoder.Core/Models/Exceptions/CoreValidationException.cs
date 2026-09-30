// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreValidationException(Exception innerException)
    : Exception("Core validation failed.", innerException);