using NATS.Client;
using StackExchange.Redis;
using System.Text;

public class Program
{
    private static readonly IConnection _natsConnection = new ConnectionFactory().CreateConnection("127.0.0.1:4222");
    private const string SERVER = "localhost";
    private const string PORT = "6379";
    static void Main(string[] args)
    {
        var subscription = _natsConnection.SubscribeAsync("RankCalculator", async (sender, args) =>
        {
            IDatabase _redisDatabase = ConnectionMultiplexer.Connect($"{SERVER}:{PORT}").GetDatabase();

            var messageBytes = args.Message.Data;
            string id = Encoding.UTF8.GetString(messageBytes);
            string? text = _redisDatabase.StringGet("TEXT-" + id);

            double rank = CalculateRank(text);

            await _redisDatabase.StringSetAsync("RANK-" + id, rank.ToString());

            Console.WriteLine($"Обработан текст: {id}, Ранг: {rank}");
        });

        Console.WriteLine("Calculate rank...");
        Console.ReadLine();
    }

    private static double CalculateRank(string text)
    {
        return (text.Count(symbol => !Char.IsLetter(symbol))) / (double)text.Length;
    }
}

