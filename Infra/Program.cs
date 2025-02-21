// Mock of the DNS infra
// A simple process pulling the state in a loop and emitting a lag metric

using Microsoft.Extensions.Logging;
using SharedModel;

// Logger
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<Program>();

// Get db + metrics
using var dbContext =
    new DatabaseContext(Environment.GetEnvironmentVariable("POSTGRE_CONNECTION_STRING")!, loggerFactory);
using var metrics = new Metrics(Environment.GetEnvironmentVariable("AI_CONNECTION_STRING")!);

int sleepTime = 1_000;
var maxChanges = 100;
int serialNumber = 0;
var tags = new[]
{
    new KeyValuePair<string, object>("site", Environment.GetEnvironmentVariable("SITE")!)
};
var replicationLag = Metrics.Meter.CreateHistogram<double>("replication.lag", "ms");
var dbQueryDuration = Metrics.Meter.CreateHistogram<double>("db.query.duration", "ms");
var dbErrors = Metrics.Meter.CreateCounter<int>("db.errors");


while (true)
{
    try
    {
        // Logging
        logger.LogInformation($"Current serial number = {serialNumber}");

        var startQueryDb = DateTime.UtcNow;
        var record = dbContext.JournalOperations.Where(op => op.OperationId > serialNumber).Take(maxChanges).ToList()
            .LastOrDefault();
        dbQueryDuration.Record((DateTime.UtcNow - startQueryDb).TotalMilliseconds);

        if (record != null)
        {
            serialNumber = record.OperationId;
            var lag = (DateTime.UtcNow - record.CreatedAt).TotalMilliseconds;
            logger.LogInformation($"Last state read {record.OperationId}, lag (ms)={lag}");
            replicationLag.Record(lag, tags!);
        }
        else
        {
            logger.LogInformation("No new record found, lag (ms)=0");
            replicationLag.Record(0, tags!);
        }
    } catch (Npgsql.PostgresException e)
    {
        logger.LogError(e, "Postgres error");
        dbErrors.Add(1);
    }

    Thread.Sleep(sleepTime);
}