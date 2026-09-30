// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreServiceException(Exception innerException)
    : Exception("The Core service failed.", innerException);