using Argus.Clients.RestClient;
using Argus.Common.Functions;
using OpenAI.Chat;
using System.Net;
using System.Text.Json.Serialization;

namespace Argus.Common.Builtin.Functions
{
    public class RestToolFunctionDescriptor : ConcreteFunctionDescriptor<Task<(HttpStatusCode HttpStatusCode, string Content)>, string, string, Dictionary<string, string>, string>
    {
        public class RestToolParametersType
        {
            [JsonPropertyName("method")]
            public string Method { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("headers")]
            public Dictionary<string, string> Headers { get; set; }

            [JsonPropertyName("body")]
            public string Body { get; set; }
        }

        public RestToolFunctionDescriptor(IRestClient restClient)
            : base(nameof(RestToolFunctionDescriptor))
        {
            _function = restClient.InvokeRestTool;

            _toolDefinition = ChatTool.CreateFunctionTool(
                "RestTool",
                "Executes a REST API call with the specified method, URL, headers, and body. Returns the response as text or JSON.",
                BinaryData.FromBytes("""
                         {
                          "type": "object",
                          "properties": {
                            "method": {
                              "type": "string",
                              "description": "The HTTP method to use (e.g., 'GET', 'POST')."
                            },
                            "url": {
                              "type": "string",
                              "description": "The URL of the REST API endpoint."
                            },
                            "headers": {
                              "type": "object",
                              "description": "The headers to include in the request.",
                              "additionalProperties": { "type": "string" }
                            },
                            "body": {
                              "type": "string",
                              "description": "The body of the request, if applicable."
                            }
                          },
                          "required": ["method", "url"],
                          "additionalProperties": false
                        }
                    """u8.ToArray()));
        }
    }
}