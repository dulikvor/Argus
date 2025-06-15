using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.Services;
using ApiTestingAgent.StateMachine.Steps;
using Argus.Clients.AzureEmbeddingClient;
using Argus.Clients.GitHubAuthentication;
using Argus.Clients.GitHubRawContentCdnClient;
using Argus.Clients.LLMQuery;
using Argus.Clients.RestClient;
using Argus.Common.Builtin.Functions;
using Argus.Common.Builtin.PromptDescriptor;
using Argus.Common.Http;
using Argus.Common.Functions;
using Argus.Common.GitHubAuthentication;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.Telemetry;
using Argus.Common.Web;
using Argus.Common.Web.Filters;
using Argus.Data;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Argus.Common.Orchestration;
using ApiTestingAgent.StateMachine;
using Argus.Common.StateMachine;

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
        services.AddControllers(options =>
        {
            options.Filters.Add(typeof(HttpContextCallContextFilter));
        });
        services.AddAuthentication(GitHubAuthenticationHandler.GitHubScheme)
            .AddScheme<GitHubAuthenticationSchemeOptions, GitHubAuthenticationHandler>(GitHubAuthenticationHandler.GitHubScheme, options => { });

        services.AddSingleton<IFunctionDescriptorFactory, FunctionDescriptorFactory>();
        services.AddSingleton<IFunctionDescriptor, GetGitHubRawContentFunctionDescriptor>();
        services.AddSingleton<IFunctionDescriptor, RestToolFunctionDescriptor>();

        services.AddSingleton<IPromptDescriptor, EndPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, CurrentStatePromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, CustomerConsentStateTransitionPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, ContextPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, ApiTestsSessionPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, ServiceInformationPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, CommandSelectPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, RestDiscoveryPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, CommandInvocationPromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, ExpectedOutcomePromptDescriptor>();
        services.AddSingleton<IPromptDescriptor, StringPromptDescriptor>();
        services.AddSingleton<IPromptDescriptorFactory, PromptDescriptorFactory>();
        services.AddSingleton<IApiTestService, ApiTestService>();
        services.AddSingleton<IOrchestrationService<ApiTestStateTransitions, StepInput>, OrchestrationService<ApiTestStateTransitions, StepInput>>();

        services.AddSingleton<IResponseStreamWriter<ServerSentEventsStreamWriter>, ServerSentEventsStreamWriter>();
        services.AddSingleton<StreamReporter>(sp =>
        {
            var streamWriter = sp.GetRequiredService<IResponseStreamWriter<ServerSentEventsStreamWriter>>();
            return new StreamReporter(streamWriter);
        });
        services.AddSingleton<ISemanticStore, SemanticStore>();

        services.AddSingleton<ITypedHttpServiceClientFactory, TypedHttpServiceClientFactory>();
        services.AddServiceHttpClient<IGitHubAuthenticationClient, GitHubAuthenticationClient, GitHubAuthenticationClientOptions>(GitHubAuthenticationClient.TokenCreator);
        services.AddServiceHttpClient<IGitHubRawContentCdnClient, GitHubRawContentCdnClient, GitHubRawContentCdnClientOptions>();
        services.AddServiceHttpClient<IRestClient, RestClient>(ignoreServerCertificateValidation: true);

        services.AddServiceHttpClient<IAzureEmbeddingClient, AzureEmbeddingClient, AzureEmbeddingClientOptions>(ApiKeyTokenProvider.TokenCreator);

        services.AddManagedServiceClient<IGitHubLLMQueryClient, GitHubLLMQueryClient>();
        services.AddManagedServiceClient<IAzureLLMQueryClient, AzureLLMQueryClient>();

        // OpenTelemetry configuration
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ApiTestingAgent"))
                    .AddSource(ActivityScope.Source)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAzureMonitorTraceExporter(options =>
                    {
                        options.ConnectionString = _configuration["AzureMonitor:ConnectionString"];
                    });
            });

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
                options.AddAzureMonitorLogExporter(o =>
                {
                    o.ConnectionString = _configuration["AzureMonitor:ConnectionString"];
                });
            })
            .AddTraceSource(ActivityScope.Source);
        });

        // Register all State-derived classes as transient
        services.AddTransient<DomainSelectionState>();
        services.AddTransient<RestDiscoveryState>();
        services.AddTransient<CommandInvocationState>();
        services.AddTransient<CommandSelectState>();
        services.AddTransient<ExpectedOutcomeState>();
        services.AddTransient<EndState<ApiTestStateTransitions, StepInput>>();
        // Register the StateFactory
        services.AddSingleton<IStateFactory, StateFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
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
        services.AddOptions<AzureLLMQueryClientOptions>()
            .Bind(_configuration.GetSection(nameof(ServiceConfiguration.AzureLLMQueryClient)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<AzureEmbeddingClientOptions>()
            .Bind(_configuration.GetSection(nameof(ServiceConfiguration.AzureEmbeddingClient)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
