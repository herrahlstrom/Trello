using System.Diagnostics;
using Newtonsoft.Json;

namespace TrelloApi
{
	[DebuggerDisplay("{Name}, {Initials}, {UserId}")]
	public class TrelloMember
	{
		[JsonProperty("avatarHash")]
		public string AvatarHash { get; set; }

		[JsonProperty("initials")]
		public string Initials { get; internal set; }

		[JsonProperty("fullname")]
		public string Name { get; internal set; }

		[JsonProperty("username")]
		public string UserId { get; internal set; }
		
		[JsonProperty("url")]
		public string Url { get; internal set; }
		
	}
}