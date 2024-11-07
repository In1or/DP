using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class Utils
    {
        public static string GetShardIdByCountry(string country)
        {
            switch (country)
            {
                case "Russia":
                    return "RUS";
                case "France":
                case "Germany":
                    return "EU";
                case "USA":
                case "India":
                    return "OTHER";
                default:
                    throw new ArgumentException("Unknown country");
            }
        }

        static void Main(string[] args)
        {
        }
    }
}
