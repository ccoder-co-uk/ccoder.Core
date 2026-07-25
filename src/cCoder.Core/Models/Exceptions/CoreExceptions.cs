// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Exceptions;

internal sealed class CoreValidationException(Exception innerException)
    : Exception("Core validation failed.", innerException);

internal sealed class CoreDependencyException(Exception innerException)
    : Exception("A Core dependency failed.", innerException);

internal sealed class CoreServiceException(Exception innerException)
    : Exception("The Core service failed.", innerException);

internal sealed class CoreProcessingValidationException(Exception innerException)
    : Exception("Core processing validation failed.", innerException);

internal sealed class CoreProcessingDependencyException(Exception innerException)
    : Exception("A Core processing dependency failed.", innerException);

internal sealed class CoreProcessingServiceException(Exception innerException)
    : Exception("The Core processing service failed.", innerException);

internal sealed class CoreOrchestrationValidationException(Exception innerException)
    : Exception("Core orchestration validation failed.", innerException);

internal sealed class CoreOrchestrationDependencyException(Exception innerException)
    : Exception("A Core orchestration dependency failed.", innerException);

internal sealed class CoreOrchestrationServiceException(Exception innerException)
    : Exception("The Core orchestration service failed.", innerException);