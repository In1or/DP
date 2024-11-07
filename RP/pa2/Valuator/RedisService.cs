using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Valuator
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _connection;
        public const string SERVER = "localhost";
        public const string PORT = "6379";

        public RedisService()
        {
            _connection = ConnectionMultiplexer.Connect($"{SERVER}:{PORT}");
        }

        public string Get(string key)
        {
            var db = _connection.GetDatabase();

            return db.StringGet(key);
        }
        public List<string> Gets()
        {
            var keys = _connection.GetServer($"{SERVER}:{PORT}").Keys();

            return keys.Select(x => x.ToString()).ToList();
        }

        public void Put(string key, string value)
        {
            var db = _connection.GetDatabase();

            db.StringSet(key, value);
        }
    }
}
