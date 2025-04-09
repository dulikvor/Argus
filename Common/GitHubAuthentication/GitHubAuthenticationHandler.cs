using Argus.Clients.GitHubAuthentication;
using Argus.Common.Clients;
using Argus.Common.Data;
using Argus.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Argus.Common.GitHubAuthentication;

public class GitHubAuthenticationHandler : AuthenticationHandler<GitHubAuthenticationSchemeOptions>
{
    public const string GitHubScheme = "GitHub";
    private readonly ITypedHttpServiceClientFactory _typedHttpServiceClientFactory;

    public GitHubAuthenticationHandler(
        IOptionsMonitor<GitHubAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ITypedHttpServiceClientFactory typedHttpServiceClientFactory)
        : base(options, logger, encoder)
    {
        _typedHttpServiceClientFactory = typedHttpServiceClientFactory;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Context.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            return AuthenticateResult.NoResult();
        }

        var token = Request.Headers["x-github-token"].ToString();
        CallContext.SetData(ServiceConstants.Authentication.GitHubTokenKey, token);
        var client = _typedHttpServiceClientFactory.Create<IGitHubAuthenticationClient, GitHubAuthenticationClient>();
        var authenticationResponse = await client.GetUserAsync();

        var claims = new[] { new Claim(ClaimTypes.Name, authenticationResponse.Name) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        Context.User = principal;

        return await Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
