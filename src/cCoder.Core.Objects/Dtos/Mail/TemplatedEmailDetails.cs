namespace cCoder.Core.Objects.Dtos.Mail;

public class TemplatedEmailDetails
{
    public string SourceDomain { get; set; }

    public string TemplateName { get; set; }

    public string Subject { get; set; }

    public string Culture { get; set; }

    public dynamic Model { get; set; }

    public string ToEmail { get; set; }
}