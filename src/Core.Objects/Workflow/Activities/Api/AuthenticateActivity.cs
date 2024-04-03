using Core.Objects.Extensions;
using System.Text.Json.Serialization;

namespace Core.Objects.Workflow.Activities.Api;

public class AuthenticateActivity : ApiActivity
{
    [JsonIgnore]
    public string Username { get; set; }

    [JsonIgnore]
    public string Password { get; set; }

    public override async Task Execute()
    {
        using var api = GetHttpClient();
        AuthToken = (await api.Authenticate(Username, Password))?.Id;
    }
}