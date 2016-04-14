using System;
using System.Runtime.Caching;

namespace TrelloApi.Cache
{
	class MemoryRequestCache : IRequestCache
	{
		private readonly MemoryCache _cache = new MemoryCache("MemoryRequestCache");
		private readonly TimeSpan _cacheTime;

		public MemoryRequestCache(TimeSpan cacheTime)
		{
			_cacheTime = cacheTime;
		}

		public string GetValue(string key)
		{
			return _cache.Get(key) as string;
		}

		public void SetValue(string key, string value)
		{
			_cache.Set(key, value, DateTimeOffset.Now.Add(_cacheTime));
		}
	}
}
