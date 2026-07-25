// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Dependencies;
namespace cCoder.Core.Services.Processings.AllowedOrigins;

internal sealed partial class AllowedOriginStoreProcessingService
{
    private static void ValidateCoreAllowedOriginOnIsAllowed(string origin) =>
        ValidationRulesEngine.Validate(inputs: [origin]);
}