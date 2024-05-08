namespace cCoder.Core.Objects.Dtos.Testing;

public abstract class ApiAction : TestAction
{
    public string BaseUrl { get; set; }
    public string Endpoint { get; set; }
    public string Query { get; set; }

    public string ResponseString { get; set; }
    public int ResponseCode { get; set; }

    public T ResponseAs<T>() => Data.ParseJson<T>(ResponseString);
}

public class GetAction : ApiAction
{
    public override async Task Execute(IDictionary<string, object> context)
    {
        HttpClient api = context["Api"] as HttpClient;
        _ = await api.GetAsync(BaseUrl + Endpoint + Query)
            .ContinueWith(async t =>
            {
                ResponseCode = (int)t.Result.StatusCode;
                ResponseString = await t.Result.Content.ReadAsStringAsync();
            });
    }
}

public class PostAction : ApiAction
{
    public string Data { get; set; }

    public override async Task Execute(IDictionary<string, object> context)
    {
        HttpClient api = context["Api"] as HttpClient;
        _ = await api.PostAsync(BaseUrl + Endpoint + Query, new StringContent(Data))
            .ContinueWith(async t =>
            {
                ResponseCode = (int)t.Result.StatusCode;
                ResponseString = await t.Result.Content.ReadAsStringAsync();
            });
    }
}

public class PutAction : ApiAction
{
    public string Data { get; set; }

    public override async Task Execute(IDictionary<string, object> context)
    {
        HttpClient api = context["Api"] as HttpClient;
        _ = await api.PutAsync(BaseUrl + Endpoint + Query, new StringContent(Data))
            .ContinueWith(async t =>
            {
                ResponseCode = (int)t.Result.StatusCode;
                ResponseString = await t.Result.Content.ReadAsStringAsync();
            });
    }
}