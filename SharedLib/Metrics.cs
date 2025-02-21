using System.Diagnostics.Metrics;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace SharedModel;

public class Metrics : IDisposable
{
    private const string MeterName = "DNSExperiment";
    public static readonly Meter Meter = new(MeterName);
    private readonly MeterProvider? _meterProvider;

    public Metrics(string connectionString)
    {
        var resourceAttributes = new Dictionary<string, object>
        {
            { "service.name", "dns-experiment" },
            { "service.namespace", "dns-experiment" }
        };

        var resourceBuilder = ResourceBuilder.CreateDefault().AddAttributes(resourceAttributes);

        _meterProvider = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddMeter(MeterName)
            .AddAzureMonitorMetricExporter(o => o.ConnectionString = connectionString)
            .Build();
    }

    public void Dispose()
    {
        _meterProvider?.Dispose();
    }
}