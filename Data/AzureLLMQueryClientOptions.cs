using Argus.Common.Http;

namespace Argus.Data;

public class AzureLLMQueryClientOptions : ServiceHttpClientOptions
{
    public string DefaultModelId { get; set; }
}