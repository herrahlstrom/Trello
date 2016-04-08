using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloApi
{
	/// <summary>
	/// Trello Options
	/// </summary>
	public class TrelloOptions
	{
		/// <summary>
		/// Token for the user accessing Trello
		/// </summary>
		public string Token { get; set; }

		/// <summary>
		/// Time every request will be cached
		/// </summary>
		public TimeSpan CacheTime { get; set; } = TimeSpan.FromMinutes(5);

		public bool PersistentCache { get; set; } = false;
	}
}
