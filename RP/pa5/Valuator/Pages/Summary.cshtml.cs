using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Utils;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IDatabase _redis;

    public SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer redisConnection)
    {
        _logger = logger;
        _redis = redisConnection.GetDatabase();
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id, string country)
    {
        _logger.LogDebug(id);
        _logger.LogDebug(country);

        //string shardId = Utils.Utils.GetShardIdByCountry(country);
        string? shardId = _redis.StringGet(id);
        string? dbConnection = Environment.GetEnvironmentVariable($"DB_{shardId}");

        if (String.IsNullOrEmpty(dbConnection))
            return;
       
        IDatabase redis = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(dbConnection)).GetDatabase();

        //TODO: проинициализировать свойства Rank и Similarity значениями из БД
        Rank = Convert.ToDouble(redis.StringGet($"RANK-{id}"));
        Similarity = Convert.ToDouble(redis.StringGet($"SIMILARITY-{id}"));
    }

}
