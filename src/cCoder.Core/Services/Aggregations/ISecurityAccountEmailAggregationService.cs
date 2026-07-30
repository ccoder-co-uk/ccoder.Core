// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Models.Events;

namespace cCoder.Core.Services.Aggregations;

internal interface ISecurityAccountEmailAggregationService
{
    ValueTask QueueRegistrationCreatedSecurityAccountEventEmailAsync(
        SecurityAccountEvent accountEvent);

    ValueTask QueueInvitationCreatedSecurityAccountEventEmailAsync(
        SecurityAccountEvent accountEvent);

    ValueTask QueuePasswordResetRequestedSecurityAccountEventEmailAsync(
        SecurityAccountEvent accountEvent);
}