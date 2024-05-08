using cCoder.Core.Objects.Dtos;
using cCoder.Core.Objects.Extensions;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;

namespace cCoder.Core.Objects.Extensions;

public static class HttpClientExtensions
{
    public static int BatchSize => 1000;

    /// <summary>
    /// Create an entity of Type T on the API
    /// </summary>
    /// <typeparam name="T">Data type being created</typeparam>
    /// <param name="client">HttpClient instance</param>
    /// <param name="query">Path to API call</param>
    /// <param name="entity">Entity to be created</param>
    /// <returns>The entity with it's populated key after creation</returns>
    public static async Task<T> AddAsync<T>(this HttpClient client, string query, T entity)
    {
        Result<T> validationResults = entity.Validate();

        if (!validationResults.Success)
            throw new ValidationException(validationResults.Message);

        HttpResponseMessage response = await client.PostAsync(query, new StringContent(entity.ToJsonForOdata(), Encoding.UTF8, "application/json"));
        _ = response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<T>();
    }

    /// <summary>
    /// Update And Entity of Type T on the API
    /// </summary>
    /// <typeparam name="T">Data type being updated</typeparam>
    /// <param name="client">HttpClient instance</param>
    /// <param name="query">Path to API call</param>
    /// <param name="entity">Entity to be created</param>
    /// <returns>The entity with it's populated key after update</returns>
    public static async Task<T> UpdateAsync<T>(this HttpClient client, string query, T entity)
    {
        Result<T> validationResults = entity.Validate();

        if (!validationResults.Success)
            throw new ValidationException(validationResults.Message);

        HttpResponseMessage response = await client.PutAsync(query, new StringContent(entity.ToJsonForOdata(), Encoding.UTF8, "application/json"));
        _ = response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<T>();
    }

    /// <summary>
    /// Delete and Entity atthe given URL on the API
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="client">HttpClient instance</param>
    /// <param name="query">Path to API call</param>
    /// <returns>Task that does the work</returns>
    public static async Task DeleteAsync(this HttpClient client, string query) => (await client.DeleteAsync(query)).EnsureSuccessStatusCode();

    /// <summary>
    /// Deletes the given set from the API
    /// </summary>
    /// <typeparam name="TKey">Type of the keys that we are sending</typeparam>
    /// <param name="client">HttpClient instance</param>
    /// <param name="query">Path to API call</param>
    /// <param name="data">Entity keys for the entities to be posted</param>
    /// <returns>Result for each key given showing what happened to each entity on the API</returns>
    public static async Task<IEnumerable<Result<T>>> PostAllAsync<T>(this HttpClient client, string query, IEnumerable<T> data)
    {
        Result<T>[] validationResults = data.Select(i => i.Validate()).ToArray();
        IEnumerable<T> validItems = validationResults.Where(r => r.Success).Select(r => r.Item);
        HttpResponseMessage response = await client.PostAsync(query, new StringContent($"{{ \"value\": {validItems.ToJsonForOdata()} }}", Encoding.UTF8, "application/json"));
        _ = response.EnsureSuccessStatusCode();
        List<Result<T>> results = (await response.Content.ReadAsAsync<ODataCollection<Result<T>>>()).Value.ToList();
        results.AddRange(validationResults.Where(r => !r.Success));
        return results;
    }

    /// <summary>
    /// Call custom Odata actions on the API
    /// </summary>
    /// <typeparam name="TResult">Expected result type</typeparam>
    /// <param name="client">HttpClient instance</param>
    /// <param name="query">Path to API call</param>
    /// <param name="data">Payload to send to the Action</param>
    /// <returns>Result for each key given showing what happened to each entity on the API</returns>
    public static async Task<TResult> CallOdataAction<TResult>(this HttpClient client, string query, object data)
    {
        HttpResponseMessage response = await client.PostAsync(query, new StringContent(data.ToJsonForOdata(), Encoding.UTF8, "application/json"));
        _ = response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<TResult>();
    }

    /// <summary>
    /// Call custom Odata actions on the API
    /// </summary>
    /// <typeparam name="TResult">Expected result type</typeparam>
    /// <param name="client">HttpClient instance</param>
    /// <param name="query">Path to API call</param>
    /// <param name="data">Payload to send to the Action</param>
    /// <returns>Task that does the work</returns>
    public static async Task CallOdataAction(this HttpClient client, string query, object data)
        => (await client.PostAsync(query, new StringContent(data.ToJsonForOdata(), Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();

    public static async Task<T> Get<T>(this HttpClient client, string query)
    {
        HttpResponseMessage response = await client.GetAsync(query);
        return await response.Content.ReadAsAsync<T>();
    }

    /// <summary>
    /// Fetches a collection of data items of type T from the API
    /// </summary>
    /// <typeparam name="TResult">Expected result type</typeparam>
    /// <param name="client">HttpClient instance</param>
    /// <param name="query">Path to API call</param>
    /// <returns>Task that resolves our Result set as an ODataCollection&gt;T&lt;</returns>
    public static async Task<IEnumerable<T>> GetODataCollection<T>(this HttpClient client, string query)
    {
        List<T> results = new();
        int page = 0;
        string fullQuery = query + (query.Contains('?') ? $"&$skip={page * BatchSize}&$top={BatchSize}" : $"?$skip={page * BatchSize}&$top={BatchSize}");

        ODataCollection<T> batch = await client.GetAsync<ODataCollection<T>>(fullQuery);
        
        while (batch?.Value?.Any() ?? false)
        {
            results.AddRange(batch.Value);
            page++;
            fullQuery = query + (query.Contains('?') ? $"&$skip={page * BatchSize}&$top={BatchSize}" : $"?$skip={page * BatchSize}&$top={BatchSize}");
            batch = await client.GetAsync<ODataCollection<T>>(fullQuery);
        }

        return results;
    }

    /// <summary>
    /// Adds the given collection of header values to an instance of a http client
    /// </summary>
    /// <param name="client">HttpClient instance</param>
    /// <param name="headers">the header values to add</param>
    /// <returns>HttpClient with the given header values</returns>
    public static HttpClient AddHeaders(this HttpClient client, NameValueCollection headers)
    {
        foreach (object key in headers.Keys)
            client.DefaultRequestHeaders.Add(key.ToString(), headers.Get(key.ToString()));

        return client;
    }

    /// <summary>
    /// Tells the HttpClient instance to use the given basic auth info in future requests (from credentials)
    /// </summary>
    /// <param name="client">HttpClient instance</param>
    /// <param name="user">Username to use when authenticating</param>
    /// <param name="pass">Password to use when authenticating</param>
    /// <returns>HttpClient instance with the auth info applied</returns>
    public static HttpClient UseBasicAuth(this HttpClient client, string user, string pass) => client
        .UseBasicAuth(Convert.ToBase64String(Encoding.UTF8.GetBytes("username=" + user + "&password=" + pass + "&grant_type=password")));

    /// <summary>
    /// Tells the HttpClient instance to use the given basic auth info in future requests (encoded)
    /// </summary>
    /// <param name="client">HttpClient instance</param>
    /// <param name="authString">Encoded Auth string to use</param>
    /// <returns>HttpClient instance with the auth info applied</returns>
    public static HttpClient UseBasicAuth(this HttpClient client, string authString) => client.WithAuthHeader("basic", authString);

    /// <summary>
    /// Sets the base Uri (scheme, domain and any port info) to use for all future requests
    /// </summary>
    /// <param name="client">HttpClient instance</param>
    /// <param name="baseUriString">The base Uri to use</param>
    /// <returns>HttpClient instance with the uri info applied</returns>
    public static HttpClient WithBaseUri(this HttpClient client, string baseUriString)
    {
        client.BaseAddress = new Uri(baseUriString);
        return client;
    }

    private static HttpClient WithAuthHeader(this HttpClient client, string authType, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authType, token);
        return client;
    }

    /// <summary>
    /// Tells the HttpClient instance to use the given auth token in future requests
    /// </summary>
    /// <param name="client">HttpClient instance</param>
    /// <param name="token">token to use</param>
    /// <returns>HttpClient instance with the auth info applied</returns>
    public static HttpClient WithAuthToken(this HttpClient client, string token) => client.WithAuthHeader("bearer", token);

    /// <summary>
    /// Get a string from the API
    /// </summary>
    /// <param name="client">HttpClient instance</param>
    /// <param name="query">Path to API call</param>
    /// <returns>The requested response as a T</returns>
    public static async Task<T> GetAsync<T>(this HttpClient client, string query)
    {
        HttpResponseMessage result = await client.GetAsync(query);
        _ = result.EnsureSuccessStatusCode();
        return await result.Content.ReadAsAsync<T>();
    }

    /// <summary>
    /// Get a string from the API
    /// </summary>
    /// <param name="client">HttpClient instance</param>
    /// <param name="query">Path to API call</param>
    /// <returns>The requested response stream</returns>
    public static Task<Stream> GetStreamAsync(this HttpClient client, string query) => client.GetAsync(query)
            .ContinueWith(t => t.Result.Content.ReadAsStreamAsync())
            .Unwrap();

    /// <summary>
    /// Get a string from the API
    /// </summary>
    /// <param name="client">HttpClient instance</param>
    /// <param name="query">Path to API call</param>
    /// <returns>The requested response string</returns>
    public static Task<string> GetStringAsync(this HttpClient client, string query) => client.GetAsync(query)
            .ContinueWith(t => t.Result.Content.ReadAsStringAsync())
            .Unwrap();
}