// Mock of the API
// A simple process pushing state in a loop and emitting a few metrics

using Microsoft.Extensions.Logging;
using SharedModel;

// Logger
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<Program>();

// Open DB connection
using var dbContext =
    new DatabaseContext(Environment.GetEnvironmentVariable("POSTGRE_CONNECTION_STRING")!, loggerFactory);
logger.LogInformation("Creating/migrating db if necessary...");
if (!dbContext.Database.EnsureCreated())
    logger.LogInformation("... db already created");
else
    logger.LogInformation("... db created");

using var metrics = new Metrics(Environment.GetEnvironmentVariable("AI_CONNECTION_STRING")!);
// Init a counter
var createdRecordsCounter = Metrics.Meter.CreateCounter<int>("created_records");

while (true)
{
    // Pick a random number of records between 1 and 100
    var records = new Random().Next(1, 100);
    logger.LogInformation($"Creating {records} records");

    // Create those records
    for (var i = 1; i <= records; i++)
    {
        var record = new JournalOperation
        {
            Operation = Enum.GetValues<Operation>()[i % 3],
            Zone = "example.com",
            RecordName = $"test{i}.example.com",
            RecordValue = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.JournalOperations.Add(record);
    }

    // Apply the changes
    dbContext.SaveChanges();

    // Emit the metric
    createdRecordsCounter.Add(records);

    // Sleep for 10 seconds
    Thread.Sleep(10_000);
}