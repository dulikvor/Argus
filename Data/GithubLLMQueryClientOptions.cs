using Argus.Common.Http;

namespace Argus.Data;

public class GitHubLLMQueryClientOptions : ServiceHttpClientOptions
{
    public string DefaultModelId { get; set; }
}