// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
namespace cCoder.Core.Services.Foundations.AllowedOrigins;

internal interface IAllowedOriginStoreService
{
    ValueTask<string[]> GetAllowedOriginsAsync();
}