// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Security;
using Microsoft.AspNetCore.Diagnostics;

namespace cCoder.Core;

public static partial class WebApplicationExtensions
{
    private static async Task HandleUnhandledException(HttpContext context)
    {
        ILogger logger = context.RequestServices
            .GetService<ILoggerFactory>()?
            .CreateLogger(categoryName: "cCoder.Core.Web")
            ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        Exception exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        context.Response.StatusCode =
            exception?.GetType() == typeof(SecurityException) ? 401 : 500;

        context.Response.ContentType = "application/json";

        if (exception is null)
        {
            return;
        }

        logger.LogError("{Message}\n{StackTrace}", exception.Message, exception.StackTrace);

        await context.Response.WriteAsync(
text: "{ \"error\": \"" + exception.Message.Replace(oldValue: "\"", newValue: "\'") + "\" }");

        Exception innerException = exception.InnerException;

        while (innerException is not null)
        {
            logger.LogError("{Message}\n{StackTrace}", innerException.Message, innerException.StackTrace);
            innerException = innerException.InnerException;
        }
    }

}