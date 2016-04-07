using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloApi
{
	public class TrelloLabel
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("idBoard")]
		public string boardId { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("color")]
		public string Color { get; set; }

		public string HexColor
		{
			get
			{
				//ToDo: lime
				switch (Color)
				{
					case "red": return "#61bd4f";
					case "green": return "#61bd4f";
					case "blue": return "#0079bf";
					case "purple": return "#c377e0";
					case "orange": return "#ffab4a";
					case "yellow": return "#f2d600";
					default: return "";
				}
			}
		}
	}
}
