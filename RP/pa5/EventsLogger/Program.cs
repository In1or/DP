using NATS.Client;
using System.Text;
using System.Text.Json;

namespace EventLogger;

public class Dto
{
    public string Id { get; set; }
    public string Data { get; set; }
}

public class Program
{
    private static readonly IConnection _natsConnection = new ConnectionFactory().CreateConnection("127.0.0.1:4222");
    static void Main(string[] args)
    {
        _natsConnection.SubscribeAsync("RankCalculated", (sender, args) =>
        {
            var message = JsonSerializer.Deserialize<Dto>(Encoding.UTF8.GetString(args.Message.Data));
            Console.WriteLine($"RankCalculated \n" +
                $"\t Id: {message.Id} \n" +
                $"\t Rank: {message.Data} \n");
        });

        _natsConnection.SubscribeAsync("SimilarityCalculated", (sender, args) =>
        {
            var message = JsonSerializer.Deserialize<Dto>(Encoding.UTF8.GetString(args.Message.Data));
            Console.WriteLine($"SimilarityCalculated \n" +
                $"\t Id: {message.Id} \n" +
                $"\t Similarity: {message.Data} \n");
        });

        Console.WriteLine("EventLogger working");
        Console.ReadLine();
    }

}
