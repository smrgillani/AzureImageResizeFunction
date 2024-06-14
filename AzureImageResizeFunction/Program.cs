using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton(x =>
        {
            var connectionString = Environment.GetEnvironmentVariable("AZURE_SA_CS");
            return new BlobServiceClient(connectionString);
        });
    })
    .Build();

host.Run();
