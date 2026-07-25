// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Core.Models.Exceptions;
using Microsoft.Data.SqlClient;

namespace cCoder.Core.Services.Foundations.Setup;

internal sealed partial class CoreSetupStateService
{
    private static async ValueTask<bool> TryCatch(
        Func<ValueTask<bool>> operation)
    {
        try
        {
            return await operation();
        }
        catch (Exception innerException)
            when (IsSetupDatabaseException(
                exception: innerException))
        {
            return false;
        }
        catch (ArgumentException innerException)
        {
            throw new CoreValidationException(
                innerException: innerException);
        }
        catch (CoreDependencyException innerException)
        {
            throw new CoreDependencyException(
                innerException: innerException);
        }
        catch (Exception innerException)
        {
            throw new CoreServiceException(
                innerException: innerException);
        }
    }

    private static bool IsSetupDatabaseException(
        Exception exception) =>
        exception switch
        {
            SqlException sqlException =>
                IsDatabaseUnavailable(
                    exception: sqlException),
            _ when exception.InnerException is not null =>
                IsSetupDatabaseException(
                    exception: exception.InnerException),
            _ => false,
        };

    private static bool IsDatabaseUnavailable(
        SqlException exception) =>
        exception.Errors
            .OfType<SqlError>()
            .Any(predicate: error =>
                error.Number is 208 or 4060 or 911);
}