using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloApi
{
	public class TrelloCard
	{
		[JsonProperty("closed")]
		public bool IsClosed { get; set; }

		[JsonProperty("desc")]
		public string Description { get; set; }

		[JsonProperty("due")]
		public DateTime? DueDate { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }

		[JsonProperty("idBoard")]
		public string BoardId { get; set; }

		[JsonProperty("idChecklists")]
		public IList<string> ChecklistIds { get; set; }

		[JsonProperty("idList")]
		public string listId { get; set; }

		[JsonProperty("idMembers")]
		public IList<string> memberIds { get; set; }

		[JsonProperty("labels")]
		public IList<TrelloLabel> Labels { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("pos")]
		public string Pos { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }
	}
}
