using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Argus.Common.Http
{
    public static class AzureAuthenticationTokenProvider
    {
        private static readonly DefaultAzureCredential Credential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ExcludeEnvironmentCredential = true,
                ExcludeManagedIdentityCredential = true,
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeVisualStudioCredential = false,
                ExcludeAzureCliCredential = true,
                ExcludeAzurePowerShellCredential = true,
                ExcludeInteractiveBrowserCredential = false
            });

        public static async Task<(string Schema, string Token)> TokenCreator(string scope, string apiKey)
        {
            var tokenRequestContext = new TokenRequestContext(new[] { scope });
            var token = await Credential.GetTokenAsync(tokenRequestContext);
            return (JwtBearerDefaults.AuthenticationScheme, token.Token);
        }
    }
}