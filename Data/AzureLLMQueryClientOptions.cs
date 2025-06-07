using Argus.Common.Clients;

namespace Argus.Data;

public class AzureLLMQueryClientOptions : ServiceHttpClientOptions
{
    public string DefaultModelId { get; set; }
}