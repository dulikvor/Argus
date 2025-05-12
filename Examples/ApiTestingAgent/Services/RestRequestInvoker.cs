using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiTestingAgent.Services
{
    public class RestRequestInvoker
    {
        private readonly HttpClient _httpClient;

        public RestRequestInvoker(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> InvokeAsync(string url, string method, object content = null)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), url);

            if (content != null)
            {
                var jsonContent = JsonSerializer.Serialize(content);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            return await _httpClient.SendAsync(request);
        }
    }
}