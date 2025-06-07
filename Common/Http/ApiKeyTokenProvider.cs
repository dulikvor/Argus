using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Argus.Common.Http
{
    public static class ApiKeyTokenProvider
    {
        public static Task<(string Schema, string Token)> TokenCreator(string scope, string apiKey)
        {
            return Task.FromResult<(string Schema, string Token)>(new (JwtBearerDefaults.AuthenticationScheme, apiKey));
        }
    }
}