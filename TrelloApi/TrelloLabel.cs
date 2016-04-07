using Newtonsoft.Json;

namespace TrelloApi
{
	public class TrelloLabel
	{
		[JsonProperty("idBoard")]
		public string BoardId { get; set; }

		[JsonProperty("color")]
		public string Color { get; set; }

		public string HexColor
		{
			get
			{
				//ToDo: lime
				switch (Color)
				{
					case "red":
						return "#61bd4f";
					case "green":
						return "#61bd4f";
					case "blue":
						return "#0079bf";
					case "purple":
						return "#c377e0";
					case "orange":
						return "#ffab4a";
					case "yellow":
						return "#f2d600";
					default:
						return "";
				}
			}
		}

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }
	}
}