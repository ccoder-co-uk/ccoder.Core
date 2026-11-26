// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using cCoder.Core.Models.Exceptions;
using cCoder.Eventing.Models.Exceptions;

namespace cCoder.Core.Services.Foundations.Eventing;

internal sealed partial class TenantEventService
{
    private static async ValueTask TryCatch(Func<ValueTask> operation)
    {
        try
        {
            await operation();
        }
        catch (LocalEventRejectedException)
        {
            throw;
        }
        catch (ArgumentException innerException)
        {
            throw new CoreValidationException(innerException: innerException);
        }
        catch (CoreDependencyException innerException)
        {
            throw new CoreDependencyException(innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new CoreServiceException(innerException: innerException);
        }
    }
}