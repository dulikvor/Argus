using Argus.Data;
using Microsoft.Extensions.Options;

namespace Argus.Clients.LLMQuery
{
    public class AzureLLMQueryClient : LLMQueryClient, IAzureLLMQueryClient
    {
        private readonly AzureLLMQueryClientOptions _clientOptions;

        public AzureLLMQueryClient(IOptions<AzureLLMQueryClientOptions> clientOptionsAccessor)
            : base(clientOptionsAccessor.Value.Endpoint, LLMProviderType.Azure)
        {
            _clientOptions = clientOptionsAccessor.Value;
        }

        protected override string GetApiKey()
        {
            return _clientOptions.ApiKey;
        }

        protected override string GetDefaultModelId()
        {
            return _clientOptions.DefaultModelId;
        }
    }
}
