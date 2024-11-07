using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

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
        if (text == ""  || text == null || string.IsNullOrEmpty(text))
            return Redirect("/");

        string id = Guid.NewGuid().ToString();

        string rankKey = "RANK-" + id;
        //TODO: посчитать rank и сохранить в БД по ключу rankKey
        var rank = (text.Length - text.Count(symbol => Char.IsLetter(symbol))) / (double)text.Length;
        _redis.Put(rankKey, rank.ToString());

        string similarityKey = "SIMILARITY-" + id;
        //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
        string similarity = _redis.Gets().Find(key => key.StartsWith("TEXT-") && _redis.Get(key) == text) != null ? "1" : "0";
        _redis.Put(similarityKey, similarity);

        string textKey = "TEXT-" + id;
        //TODO: сохранить в БД text по ключу textKey
        _redis.Put(textKey, text);
        
        return Redirect($"summary?id={id}");
    }

}
