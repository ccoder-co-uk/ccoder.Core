// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace Web.Models.Exceptions;

internal sealed class ApiScriptServiceException(
    Exception innerException)
    : Exception("The API script service failed.", innerException);