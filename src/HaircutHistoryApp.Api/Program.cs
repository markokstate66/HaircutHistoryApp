using HaircutHistoryApp.Api.Middleware;
using HaircutHistoryApp.Api.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.UseMiddleware<AuthMiddleware>();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Configure Cosmos DB
        // Note: Azure App Settings use __ which translates to : in configuration
        var cosmosConnectionString = context.Configuration["CosmosDb:ConnectionString"];
        var cosmosDatabaseName = context.Configuration["CosmosDb:DatabaseName"];

        services.AddSingleton(sp =>
        {
            var options = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };
            return new CosmosClient(cosmosConnectionString, options);
        });

        services.AddSingleton<ICosmosDbService>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            return new CosmosDbService(client, cosmosDatabaseName!);
        });

        // Configure Blob Storage
        var blobConnectionString = context.Configuration["BlobStorage:ConnectionString"];
        var blobContainerName = context.Configuration["BlobStorage:ContainerName"];
        services.AddSingleton<IBlobService>(sp => new BlobService(blobConnectionString!, blobContainerName!));

        // Configure settings
        services.Configure<FreeTierSettings>(options =>
        {
            options.MaxProfiles = int.Parse(context.Configuration["FreeTier:MaxProfiles"] ?? "1");
            options.MaxHaircutsPerProfile = int.Parse(context.Configuration["FreeTier:MaxHaircutsPerProfile"] ?? "3");
        });
    })
    .Build();

host.Run();

/// <summary>
/// Free tier limits configuration.
/// </summary>
public class FreeTierSettings
{
    public int MaxProfiles { get; set; } = 1;
    public int MaxHaircutsPerProfile { get; set; } = 3;
}
