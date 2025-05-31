using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Argus.Common.Clients
{
    public static class AzureAuthenticationTokenProvider
    {
        private static readonly DefaultAzureCredential Credential = new DefaultAzureCredential();

        public static async Task<(string Schema, string Token)> TokenCreator(string scope)
        {
            var tokenRequestContext = new TokenRequestContext(new[] { scope });
            var token = await Credential.GetTokenAsync(tokenRequestContext);
            return (JwtBearerDefaults.AuthenticationScheme, token.Token);
        }
    }
}