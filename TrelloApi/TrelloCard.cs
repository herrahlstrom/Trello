using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace TrelloApi
{
	[DebuggerDisplay("{Name}")]
	public class TrelloCard
	{
		[JsonProperty("idBoard")]
		public string BoardId { get; set; }

		[JsonProperty("id")]
		public string CardId { get; set; }

		[JsonProperty("idChecklists")]
		public IList<string> ChecklistIds { get; set; }

		[JsonProperty("desc")]
		public string Description { get; set; }

		[JsonProperty("due")]
		public DateTime? DueDate { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }

		[JsonProperty("closed")]
		public bool IsClosed { get; set; }

		[JsonProperty("labels")]
		public IList<TrelloLabel> Labels { get; set; }

		[JsonProperty("idList")]
		public string ListId { get; set; }

		[JsonProperty("idMembers")]
		public IList<string> MemberIds { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("pos")]
		public string Pos { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }
	}
}