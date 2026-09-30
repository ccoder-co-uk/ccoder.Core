// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreOrchestrationDependencyException(Exception innerException)
    : Exception("A Core orchestration dependency failed.", innerException);