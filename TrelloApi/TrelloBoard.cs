﻿using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace TrelloApi
{
	[DebuggerDisplay("{Name}")]
	public class TrelloBoard : IComparable<TrelloBoard>
	{
		[JsonProperty("id")]
		public string Id { get; internal set; }

		[JsonProperty("closed")]
		public bool IsClosed { get; internal set; }

		[JsonProperty("starred")]
		public bool IsStarred { get; internal set; }

		[JsonProperty("name")]
		public string Name { get; internal set; }

		[JsonProperty("idOrganization")]
		public string OrganizationId { get; internal set; }

		public int CompareTo(TrelloBoard other)
		{
			return string.Compare(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}