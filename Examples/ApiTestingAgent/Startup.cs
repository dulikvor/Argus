using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.Services;
using Argus.Clients.GitHubAuthentication;
using Argus.Clients.GitHubLLMQuery;
using Argus.Clients.GitHubRawContentCdnClient;
using Argus.Common.Builtin.Functions;
using Argus.Common.Clients;
using Argus.Common.Functions;
using Argus.Common.GitHubAuthentication;
using Argus.Common.PromptDescriptors;
using Argus.Common.Web;
using Argus.Data;

namespace ApiTestingAgent;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        ConfigurationBinding(services);
        services.AddControllers();
        services.AddAuthentication(GitHubAuthenticationHandler.GitHubScheme)
            .AddScheme<GitHubAuthenticationSchemeOptions, GitHubAuthenticationHandler>(GitHubAuthenticationHandler.GitHubScheme, options => { });

        services.AddSingleton<IFunctionDescriptorFactory, FunctionDescriptorFactory>();
        services.AddSingleton<IFunctionDescriptor, GetGitHubRawContentFunctionDescriptor>();

        services.AddSingleton<IStatePromptDescriptor, ServiceInformationPromptDescriptor>();
        services.AddSingleton<IStatePromptDescriptor, RestDiscoveryPromptDescriptor>();
        services.AddSingleton<IPromptDescriptorFactory, PromptDescriptorFactory>();
        services.AddSingleton<IApiTestService, ApiTestService>();

        services.AddSingleton<IResponseStreamWriter<ServerSentEventsStreamWriter>, ServerSentEventsStreamWriter>();

        services.AddSingleton<ITypedHttpServiceClientFactory, TypedHttpServiceClientFactory>();
        services.AddServiceHttpClient<IGitHubAuthenticationClient, GitHubAuthenticationClient, GitHubAuthenticationClientOptions>(GitHubAuthenticationClient.TokenCreator);
        services.AddServiceHttpClient<IGitHubRawContentCdnClient, GitHubRawContentCdnClient, GitHubRawContentCdnClientOptions>();
        services.AddManagedServiceClient<IGitHubLLMQueryClient, GitHubLLMQueryClient>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private void ConfigurationBinding(IServiceCollection services)
    {
        services.AddOptions<GitHubAuthenticationClientOptions>()
            .Bind(_configuration.GetSection(nameof(ServiceConfiguration.GitHubAuthenticationClient)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<GitHubRawContentCdnClientOptions>()
            .Bind(_configuration.GetSection(nameof(ServiceConfiguration.GitHubRawContentCdnClient)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<GitHubLLMQueryClientOptions>()
            .Bind(_configuration.GetSection(nameof(ServiceConfiguration.GitHubLLMQueryClient)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
