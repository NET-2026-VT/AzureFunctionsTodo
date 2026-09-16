using Azure.Data.Tables;
using Azure.Monitor.OpenTelemetry.Exporter;
using AzureFunctionsTodo.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();


var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("AzureWebJobsStorage connection string is not configured"); 
}

builder.Services.AddSingleton(new TableServiceClient(connectionString));

builder.Services.AddScoped<ITodoService, TodoService>(); 


if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
