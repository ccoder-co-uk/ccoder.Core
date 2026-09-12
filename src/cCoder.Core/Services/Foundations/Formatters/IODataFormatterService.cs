// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Services.Foundations.Formatters;

internal interface IFormatterODataService
{
    object UnpackSelectExpandWrapper(object contextObject);
}