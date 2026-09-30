// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
namespace Web.Models.Exceptions;

internal sealed class ApiScriptDependencyException(
    Exception innerException)
    : Exception("An API script dependency failed.", innerException);