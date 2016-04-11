using System.Collections.Generic;

namespace TrelloWindow.PrintCompactModels
{
	internal class Card
	{
		public Board Board { get; set; }
		public string Description { get; set; }
		public string DueDate { get; set; }
		public IEnumerable<Label> Labels { get; set; }
		public BoardList List { get; set; }
		public IEnumerable<Member> Members { get; set; }
		public string Name { get; set; }
	}
}