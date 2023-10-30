using System.Globalization;
using CsvHelper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

public class Location
{
  public string type { get; set; } = "Point";
  public double[] coordinates { get; set; } = { };
}

public class Site
{
  public string id { get; set; } = String.Empty;
  public string siteId { get; set; } = String.Empty;
  public string address { get; set; } = String.Empty;
  public double lat { get; set; }
  public double lon { get; set; }
  public string city { get; set; } = String.Empty;
  public string province { get; set; } = String.Empty;
  public Location location { get; set; } = new Location();
}

public class CsvFile
{
  public string siteId { get; set; } = String.Empty;
  public string city { get; set; } = String.Empty;
  public string province { get; set; } = String.Empty;
  public string address { get; set; } = String.Empty;
  public double lat { get; set; }
  public double lon { get; set; }
}

public class Program
{
  // TODO: should be pulling from config instead of hard coding...
  private string EndpointUrl = "";
  private string AuthorizationKey = "";
  private string DatabaseName = "sites";
  private string ContainerName = "sites";

  public static IConfigurationRoot? Configuration;

  static async Task Main(string[] args)
  {
    var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables();

    Configuration =e builder.Build();
    var records = ImportDataFromFile();
    await PublishDataToCosmosDB(records);
  }

  public static async Task PublishDataToCosmosDB(IList<CsvFile> records)
  {

    CosmosClient cosmosClient = new CosmosClient(EndpointUrl, AuthorizationKey, new CosmosClientOptions() { AllowBulkExecution = true });
    Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(Program.DatabaseName);
    await database.DefineContainer(Program.ContainerName, "/siteId")
          .WithIndexingPolicy()
              .WithIndexingMode(IndexingMode.Consistent)
              .WithIncludedPaths()
                  .Attach()
              .WithExcludedPaths()
                  .Path("/*")
                  .Attach()
          .Attach().CreateIfNotExistsAsync();
    Console.WriteLine("Setting CosmosDB Max Throughput to 5000 RU");
    await database.ReplaceThroughputAsync(ThroughputProperties.CreateManualThroughput(5000));
    Container container = database.GetContainer(ContainerName);
    var totalRecords = records.Count;
    List<Task> tasks = new List<Task>(totalRecords);
    foreach (var (record, i) in records.Select((value, i) => (value, i)))
    {

      {
        var site = new Site
        {
          id = record.siteId,
          siteId = record.siteId,
          address = record.address,
          city = record.city,
          province = record.province,
          location = new Location
          {
            type = "Point",
            coordinates = new double[] { record.lon, record.lat }
          }

        };
        tasks.Add(container.UpsertItemAsync(site, new PartitionKey(site.siteId))
          .ContinueWith(itemResponse =>
          {

            if (!itemResponse.IsCompletedSuccessfully)
            {
              AggregateException innerExceptions = itemResponse.Exception.Flatten();
              if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
              {
                Console.WriteLine($"Received {cosmosException.StatusCode} ({cosmosException.Message}).");
              }
              else
              {
                Console.WriteLine($"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
              }
            }
          }));
      }
    }

    var start = DateTime.Now;
    Console.Write("Processing Sites");
    Task entireTask = Task.WhenAll(tasks);
    while (await Task.WhenAny(entireTask, Task.Delay(1000)) != entireTask)
    {
      Console.Write(".");
    }
    var timeDiff = DateTime.Now - start;
    Console.WriteLine("\nOperation completed in {0} seconds.", timeDiff.TotalSeconds);
    Console.WriteLine("Updating Throughput back to 500 RU");
    await database.ReplaceThroughputAsync(ThroughputProperties.CreateManualThroughput(500));
  }

  public static IList<CsvFile> ImportDataFromFile()
  {
    IList<CsvFile> records;
    using (var reader = new StreamReader("./resources/site_ids.csv"))
    {
      using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
      {
        records = csv.GetRecords<CsvFile>().ToList();
      }
    }
    return records;
  }
}