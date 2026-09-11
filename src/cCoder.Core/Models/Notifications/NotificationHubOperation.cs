// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

namespace cCoder.Core.Models.Notifications;

internal sealed class NotificationHubOperation
{
    public NotificationHubOperationKind Kind { get; set; }
    public object Groups { get; set; }
    public object Clients { get; set; }
    public string ConnectionId { get; set; }
    public string Thread { get; set; }
    public string Method { get; set; }
    public object[] Arguments { get; set; }
}