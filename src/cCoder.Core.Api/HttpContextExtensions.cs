namespace cCoder.Core.Api;

public static class HttpContextExtensions
{
    public static string GetQueryParameter(this HttpContext context, string key) =>
        context.Request.Query[key].ToString();
}