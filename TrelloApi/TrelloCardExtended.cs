using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
