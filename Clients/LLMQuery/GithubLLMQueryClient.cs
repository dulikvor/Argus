using Argus.Clients.LLMQuery;
using Argus.Common.Data;
using Argus.Data;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;

namespace Argus.Clients.LLMQuery
{
    public class GitHubLLMQueryClient : LLMQueryClient, IGitHubLLMQueryClient
    {
        private readonly GitHubLLMQueryClientOptions _clientOptions;

        public GitHubLLMQueryClient(IOptions<GitHubLLMQueryClientOptions> clientOptionsAccessor)
            : base(clientOptionsAccessor.Value.Endpoint, LLMProviderType.OpenAI)
        {
            _clientOptions = clientOptionsAccessor.Value;
        }

        protected override string GetDefaultModelId()
        {
            return _clientOptions.DefaultModelId;
        }

        protected override string GetApiKey()
        {
            return CallContext.GetData(ServiceConstants.Authentication.GitHubTokenKey) as string;
        }
    }
}
