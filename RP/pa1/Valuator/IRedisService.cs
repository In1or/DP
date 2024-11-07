using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace Valuator
{
    public interface IRedisService
    {
        public List<string> Gets();
        public string Get(string key);
        public void Put(string key, string value);
    }
}
