using NATS.Client;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using Utils;

namespace RankCalculator;

public class Dto
{
    public string Id { get; set; }
    public string Data { get; set; }
}

public class Program
{
    private static readonly IConnection _natsConnection = new ConnectionFactory().CreateConnection("127.0.0.1:4222");
    private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");

    static void Main(string[] args)
    {
        var subscription = _natsConnection.SubscribeAsync("RankCalculator", async (sender, args) =>
        {
            string messageBytes = Encoding.UTF8.GetString(args.Message.Data);
            Dto? structData = JsonSerializer.Deserialize<Dto>(messageBytes);

            //string shardId = Utils.Utils.GetShardIdByCountry(structData.Data);

            string? shardId = redis.GetDatabase().StringGet(structData.Id);
            string? redisConnection = Environment.GetEnvironmentVariable($"DB_{shardId}");
            IDatabase _redisDatabase = ConnectionMultiplexer.Connect(redisConnection).GetDatabase();

            string? text = _redisDatabase.StringGet("TEXT-" + structData.Id);

            double rank = CalculateRank(text);

            await _redisDatabase.StringSetAsync("RANK-" + structData.Id, rank.ToString());
            Console.WriteLine($"LOOKUP: {structData.Id}, {structData.Data}");

            PublishRankCalculatedEvent(structData.Id, rank.ToString());
        });

        Console.WriteLine("Calculate rank...");
        Console.ReadLine();
    }

    private static double CalculateRank(string text)
    {
        return (text.Count(symbol => !Char.IsLetter(symbol))) / (double)text.Length;
    }

    private static void PublishRankCalculatedEvent(string id, string rank)
    {
        var message = new { Id = id, Data = rank };
        string messageJson = JsonSerializer.Serialize(message);
        _natsConnection.Publish("RankCalculated", Encoding.UTF8.GetBytes(messageJson));
    }
}

