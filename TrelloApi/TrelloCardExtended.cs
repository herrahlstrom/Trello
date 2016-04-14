using Newtonsoft.Json;

namespace TrelloApi
{
	public class TrelloCardExtended : TrelloCard
	{
		[JsonProperty("board")]
		public TrelloBoard Board { get; set; }

		[JsonProperty("list")]
		public TrelloBoardList List { get; set; }
	}
}
