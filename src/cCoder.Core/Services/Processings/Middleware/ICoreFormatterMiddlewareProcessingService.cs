// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
namespace cCoder.Core.Services.Processings.Middleware;

internal interface ICoreFormatterMiddlewareProcessingService
{
    Task ProcessAsync(
        HttpContext context,
        RequestDelegate next);
}