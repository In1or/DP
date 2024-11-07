using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NATS.Client;
using System.Text;
using System.Text.Json;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IRedisService _redis;


    public IndexModel(ILogger<IndexModel> logger, IRedisService redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);
        if (string.IsNullOrEmpty(text))
            return Redirect("/");

        string id = Guid.NewGuid().ToString();

        Options options = ConnectionFactory.GetDefaultOptions();
        options.Url = "127.0.0.1:4222";
        IConnection _natsConnection = new ConnectionFactory().CreateConnection(options);

        string similarityKey = "SIMILARITY-" + id;
        string similarity = _redis.Gets().Find(key => key.StartsWith("TEXT-") && _redis.Get(key) == text) != null ? "1" : "0";
        _redis.Put(similarityKey, similarity);

        var message = new { Id = id, Data = similarity };
        string messageJson = JsonSerializer.Serialize(message);
        _natsConnection.Publish("SimilarityCalculated", Encoding.UTF8.GetBytes(messageJson));

        string textKey = "TEXT-" + id;
        _redis.Put(textKey, text);

        _natsConnection.Publish("RankCalculator", Encoding.UTF8.GetBytes(id));

        System.Threading.Thread.Sleep(1000);

        return Redirect($"summary?id={id}");
    }

}
