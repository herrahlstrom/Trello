using Newtonsoft.Json;

namespace TrelloApi
{
	public class BoardList
	{
		[JsonProperty("closed")]
		public string Closed { get; set; }

		[JsonProperty("idBoard")]
		public string BoardId { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("pos")]
		public string Pos { get; set; }
	}
}
