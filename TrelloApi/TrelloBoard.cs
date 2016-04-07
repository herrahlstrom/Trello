using Newtonsoft.Json;

namespace TrelloApi
{
	public class TrelloBoard
	{
		[JsonProperty("id")]
		public string BoardId { get; internal set; }

		[JsonProperty("closed")]
		public bool IsClosed { get; internal set; }

		[JsonProperty("starred")]
		public bool IsStarred { get; internal set; }

		[JsonProperty("name")]
		public string Name { get; internal set; }

		[JsonProperty("idOrganization")]
		public string OrganizationId { get; internal set; }
	}
}