// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Objects.Events;

namespace cCoder.Core.Services.Orchestrations;

public interface ISecurityAccountEmailOrchestrationService
{
    ValueTask QueueRegistrationCreatedEmailAsync(SecurityAccountEvent accountEvent);

    ValueTask QueueInvitationCreatedEmailAsync(SecurityAccountEvent accountEvent);

    ValueTask QueuePasswordResetRequestedEmailAsync(SecurityAccountEvent accountEvent);
}