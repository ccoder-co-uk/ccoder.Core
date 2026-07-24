// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Objects.Events;

namespace cCoder.Core.Services.Aggregations;

public interface ISecurityAccountEmailAggregationService
{
    ValueTask QueueRegistrationCreatedSecurityAccountEventEmailAsync(
        SecurityAccountEvent accountEvent);

    ValueTask QueueInvitationCreatedSecurityAccountEventEmailAsync(
        SecurityAccountEvent accountEvent);

    ValueTask QueuePasswordResetRequestedSecurityAccountEventEmailAsync(
        SecurityAccountEvent accountEvent);
}