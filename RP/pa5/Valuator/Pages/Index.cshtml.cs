using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NATS.Client;
using StackExchange.Redis;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.Json;
using Utils;
using static System.Net.Mime.MediaTypeNames;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDatabase _redis;

    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redisConnection)
    {
        _logger = logger;
        _redis = redisConnection.GetDatabase();
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text, string country)
    {
        _logger.LogDebug(text);
        _logger.LogDebug(country);

        string id = Guid.NewGuid().ToString();
        Console.WriteLine(id);
        string shardId = Utils.Utils.GetShardIdByCountry(country);
        Console.WriteLine(shardId);
        Console.WriteLine(Environment.GetEnvironmentVariable($"DB_{shardId}"));
        string redisConnection = Environment.GetEnvironmentVariable($"DB_{shardId}");

        if (string.IsNullOrEmpty(text) && redisConnection != null)
            return Redirect("/");       

        _redis.StringSet(id, shardId);

        ConfigurationOptions redisConfiguration = ConfigurationOptions.Parse(redisConnection);
        IConnectionMultiplexer redisDB = ConnectionMultiplexer.Connect(redisConfiguration);

        Options options = ConnectionFactory.GetDefaultOptions();
        options.Url = "127.0.0.1:4222";
        IConnection _natsConnection = new ConnectionFactory().CreateConnection(options);

        string similarityKey = "SIMILARITY-" + id;
        //string similarity = redisDB.GetServer(redisConnection).Keys().Select(x => x.ToString())
        //    .ToList().Find(key => key.StartsWith("TEXT-") && redisDB.GetDatabase().StringGet(key) == text) != null ? "1" : "0";

        var similarity = GetSimilarity(text);

        redisDB.GetDatabase().StringSet(similarityKey, similarity);
        Console.WriteLine($"LOOKUP: {id}, {country}");

        var message = new { Id = id, Data = similarity };
        string messageJson = JsonSerializer.Serialize(message);
        _natsConnection.Publish("SimilarityCalculated", Encoding.UTF8.GetBytes(messageJson));

        string textKey = "TEXT-" + id;
        redisDB.GetDatabase().StringSet(textKey, text);
        Console.WriteLine($"LOOKUP: {id}, {country}");

        var dataCountryAndText = new { Id = id, Data = country };
        string dataCountryAndTexJson = JsonSerializer.Serialize(dataCountryAndText);
        _natsConnection.Publish("RankCalculator", Encoding.UTF8.GetBytes(dataCountryAndTexJson));

        System.Threading.Thread.Sleep(1000);

        return Redirect($"summary?id={id}&country={country}");
    }

    private static string GetSimilarity(string text)
    {
        var a = Environment.GetEnvironmentVariables();
        string similarity = "";

        foreach (var key in a.Keys ) 
        {
            if (key.ToString().StartsWith("DB_"))
            {
                ConfigurationOptions redisConfiguration = ConfigurationOptions.Parse(a[key].ToString());
                IConnectionMultiplexer redisDB = ConnectionMultiplexer.Connect(redisConfiguration);

                similarity = redisDB.GetServer(a[key].ToString()).Keys().Select(x => x.ToString())
                    .ToList().Find(key => key.StartsWith("TEXT-") && redisDB.GetDatabase().StringGet(key) == text) != null ? "1" : "0";
                if (similarity == "1")
                    break;
            }
        }
        return similarity;
    }
}
