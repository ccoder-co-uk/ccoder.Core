namespace cCoder.Core.Objects.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class PortalAdminRequiredToAttribute : Attribute
{
    public string PrivKey { get; set; }

    public PortalAdminRequiredToAttribute(string privKey)
    {
        PrivKey = privKey;
    }
}
