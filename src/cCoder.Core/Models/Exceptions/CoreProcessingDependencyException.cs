// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreProcessingDependencyException(Exception innerException)
    : Exception("A Core processing dependency failed.", innerException);