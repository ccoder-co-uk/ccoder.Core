using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace cCoder.Core.Objects.Workflow.Activities.Api;

public class AuthenticateActivity : ApiActivity
{
    record Token(
        string Id, 
        int Reason, 
        DateTimeOffset Expires, 
        string UserName);

    [JsonIgnore]
    public string Username { get; set; }

    [JsonIgnore]
    public string Password { get; set; }

    public override async Task Execute()
    {
        using HttpClient api = GetHttpClient();

        var auth = new { User = Username, Pass = Password };
        HttpResponseMessage response = await api.PostAsync("Account/Login", new StringContent(Json(auth), Encoding.UTF8, "application/json"));
        _ = response.EnsureSuccessStatusCode();
        Token token = await ReadAsAsync<Token>(response.Content);
        AuthToken = token.Id;
    }

    public static async Task<T> ReadAsAsync<T>(HttpContent content)
        => JsonConvert.DeserializeObject<T>(await content.ReadAsStringAsync());

    static string Json(object source) 
        => JsonConvert.SerializeObject(source, new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        TypeNameHandling = TypeNameHandling.None,
        Formatting = Formatting.None,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        ContractResolver = new DefaultContractResolver { IgnoreSerializableAttribute = true },
        MaxDepth = 4
    });
}