using Newtonsoft.Json;

namespace TrelloApi
{
	public class BoardList
	{
		[JsonProperty("closed")]
		public string Closed { get; set; }

		/// <summary>
		/// Id of the board that this list is associated to
		/// </summary>
		[JsonProperty("idBoard")]
		public string BoardId { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("pos")]
		public float Pos { get; set; }
	}
}
