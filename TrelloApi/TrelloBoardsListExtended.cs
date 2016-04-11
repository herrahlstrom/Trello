using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TrelloApi
{
	public class TrelloBoardsListExtended : TrelloBoardList
	{
		[JsonProperty("board")]
		public TrelloBoard Board { get; set; }
	}
}
