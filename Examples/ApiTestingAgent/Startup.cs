using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.Services;
using Argus.Clients.AzureEmbeddingClient;
using Argus.Clients.GitHubAuthentication;
using Argus.Clients.GitHubLLMQuery;
using Argus.Clients.GitHubRawContentCdnClient;
using Argus.Clients.RestClient;
using Argus.Common.Builtin.Functions;
using Argus.Common.Builtin.PromptDescriptor;
using Argus.Common.Clients;
using Argus.Common.Functions;
using Argus.Common.GitHubAuthentication;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
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
        services.AddSingleton<IFunctionDescriptor, RestToolFunctionDescriptor>();

        services.AddSingleton<IPromptDescriptor, EndPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, CurrentStatePromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, CustomerConsentStateTransitionPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, ContextPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, ApiTestsPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, ServiceInformationPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, CommandDiscoveryPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, RestDiscoveryPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, CommandInvocationPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, ExpectedOutcomePromptDescriptor>();
        services.AddSingleton<IPromptDescriptorFactory, PromptDescriptorFactory>();
        services.AddSingleton<IApiTestService, ApiTestService>();

        services.AddSingleton<IResponseStreamWriter<ServerSentEventsStreamWriter>, ServerSentEventsStreamWriter>();
        services.AddSingleton<ISemanticStore, SemanticStore>();

        services.AddSingleton<ITypedHttpServiceClientFactory, TypedHttpServiceClientFactory>();
        services.AddServiceHttpClient<IGitHubAuthenticationClient, GitHubAuthenticationClient, GitHubAuthenticationClientOptions>(GitHubAuthenticationClient.TokenCreator);
        services.AddServiceHttpClient<IGitHubRawContentCdnClient, GitHubRawContentCdnClient, GitHubRawContentCdnClientOptions>();
        services.AddServiceHttpClient<IRestClient, RestClient>(ignoreServerCertificateValidation: true);

        services.AddServiceHttpClient<IAzureEmbeddingClient, AzureEmbeddingClient, AzureEmbeddingClientOptions>(AzureAuthenticationTokenProvider.TokenCreator);

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
        services.AddOptions<AzureEmbeddingClientOptions>()
            .Bind(_configuration.GetSection(nameof(ServiceConfiguration.AzureEmbeddingClient)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
