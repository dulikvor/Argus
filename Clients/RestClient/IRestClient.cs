namespace Argus.Clients.RestClient
{
    public interface IRestClient
    {
        Task<(int HttpStatusCode, string Content)> InvokeRestTool(string method, string url, Dictionary<string, string> headers, string body);
    }
}