using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace TrelloApi
{
	[DebuggerDisplay("{Name}")]
	public class TrelloChecklist
	{
		[JsonProperty("idBoard")]
		public string BoardId { get; set; }

		[JsonProperty("idCard")]
		public string CardId { get; set; }

		[JsonProperty("checkItems")]
		public IList<TrelloChecklistItem> Items { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("pos")]
		public string Pos { get; set; }
	}

	[DebuggerDisplay("{Name} - {IsComplete}")]
	public class TrelloChecklistItem
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		public bool IsComplete => string.CompareOrdinal(State, "complete") == 0;

		[JsonProperty("pos")]
		public string Pos { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }
	}
}