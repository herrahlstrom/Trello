using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace TrelloApi
{
	/// <summary>
	///     Member in trello
	/// </summary>
	[DebuggerDisplay("{Name}, {Initials}, {Id}")]
	public class TrelloMember : IComparable<TrelloMember>, IEquatable<TrelloMember>
	{
		[JsonProperty("avatarHash")]
		public string AvatarHash { get; set; }

		public string Avatar30Px => string.IsNullOrWhiteSpace(AvatarHash) ? "" : $"https://trello-avatars.s3.amazonaws.com/{AvatarHash}/30.png";

		/// <summary>
		///     User if of the member
		/// </summary>
		[JsonProperty("username")]
		public string Id { get; internal set; }

		/// <summary>
		///     MA for Martin Ahlström
		/// </summary>
		[JsonProperty("initials")]
		public string Initials { get; internal set; }

		/// <summary>
		///     Name of the member
		/// </summary>
		[JsonProperty("fullname")]
		public string Name { get; internal set; }

		/// <summary>
		///     Direct url to the members page on trello
		/// </summary>
		[JsonProperty("url")]
		public string Url { get; internal set; }

		public int CompareTo(TrelloMember other)
		{
			return string.Compare(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
		}

		public bool Equals(TrelloMember other)
		{
			if (string.IsNullOrWhiteSpace(Id))
				return false;
			return string.Compare(Id, other.Id, StringComparison.InvariantCultureIgnoreCase) == 0;
		}
	}
}