using System;

namespace TrelloApi
{
	/// <summary>
	///     Trello Options
	/// </summary>
	public class TrelloOptions : ICloneable
	{
		/// <summary>
		///     Time every request will be cached
		/// </summary>
		public TimeSpan CacheTime { get; set; } = TimeSpan.FromMinutes(5);

		public bool PersistentCache { get; set; }

		/// <summary>
		///     Token for the user accessing Trello
		/// </summary>
		public string Token { get; set; }

		public object Clone()
		{
			return new TrelloOptions
			{
				Token = Token,
				CacheTime = CacheTime,
				PersistentCache = PersistentCache
			};
		}
	}
}