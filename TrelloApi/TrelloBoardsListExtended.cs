using Newtonsoft.Json;

namespace TrelloApi
{
	public class TrelloBoardsListExtended : TrelloBoardList
	{
		[JsonProperty("board")]
		public TrelloBoard Board { get; set; }
	}
}
