using Argus.Common.Clients;
using Argus.Common.Data;
using System.Text;

namespace Argus.Clients.RestClient
{
    public class RestClient : IRestClient
    {
        private readonly HttpClient _httpClient;

        public RestClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(int HttpStatusCode, string Content)> InvokeRestTool(string method, string url, Dictionary<string, string> headers, string body)
        {
            ArgumentValidationHelper.Ensure.NotNull(method, nameof(method));
            ArgumentValidationHelper.Ensure.NotNull(url, nameof(url));

            var request = new HttpRequestMessage(new HttpMethod(method), url);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrEmpty(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return ((int)response.StatusCode, content);
        }
    }
}