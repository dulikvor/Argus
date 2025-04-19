using Argus.Clients.GitHubRawContentCdnClient;
using Argus.Common.Functions;
using OpenAI.Chat;
using System.Text.Json.Serialization;

namespace Argus.Common.Builtin.Functions
{
    public class GetGitHubRawContentFunctionDescriptor : ConcreteFunctionDescriptor<Task<string>, string, string, string, string>
    {
        private readonly IGitHubRawContentCdnClient _gitHubRawContentCdnClient;

        public class GetGitHubRawContentParametersType
        {
            [JsonPropertyName("user")]
            public string User { get; set; }

            [JsonPropertyName("repo")]
            public string Repo { get; set; }

            [JsonPropertyName("branch")]
            public string Branch { get; set; }

            [JsonPropertyName("pathToFile")]
            public string PathToFile { get; set; }
        }


        public GetGitHubRawContentFunctionDescriptor(IGitHubRawContentCdnClient gitHubRawContentCdnClient)
            : base(nameof(GetGitHubRawContentFunctionDescriptor))
        {
            _gitHubRawContentCdnClient = gitHubRawContentCdnClient;
            _function = _gitHubRawContentCdnClient.GetRawContent;

            _toolDefinition = ChatTool.CreateFunctionTool(
                "GetGitHubRawContent",
                "Fetches the raw content of a file from a GitHub repository using its raw URL. This function performs an HTTP GET request to the provided GitHub raw content URL and returns the response body as text or JSON, depending on the file type.",
                BinaryData.FromBytes(""" 
                         {
                          "type": "object",
                          "properties": {
                            "user": {
                              "type": "string",
                              "description": "The GitHub username or organization name."
                            },
                            "repo": {
                              "type": "string",
                              "description": "The name of the GitHub repository."
                            },
                            "branch": {
                              "type": "string",
                              "description": "The branch name (e.g., 'main' or 'master')."
                            },
                            "pathToFile": {
                              "type": "string",
                              "description": "The path to the file within the repository (e.g., 'folder/file.json')."
                            }
                          },
                          "required": ["user", "repo", "branch", "pathToFile"],
                          "additionalProperties": false
                        }
                    """u8.ToArray()));
        }
    }
}
