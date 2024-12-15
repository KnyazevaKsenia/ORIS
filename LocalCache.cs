using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public static class LocalCache
    {
        public static Dictionary<string, CacheItem> Cache = new();

    }
}
