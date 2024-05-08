namespace cCoder.Core.Objects;

public class QueueMessage
{
    public string Event { get; set; }
    public string ObjectJson { get; set; }
    public MessageAuthInfo Auth { get; set; }
}

public class MessageAuthInfo : ICoreAuthInfo
{
    public string SSOUserId { get; set; }

    public string Token { get; set; }

    public MessageAuthInfo()
    {

    }

    public MessageAuthInfo(ICoreAuthInfo source)
    {
        SSOUserId = source.SSOUserId;
    }
}