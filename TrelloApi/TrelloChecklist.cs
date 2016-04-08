using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace TrelloApi
{
	/// <summary>
	/// Checklist on a card
	/// </summary>
	[DebuggerDisplay("{Name}")]
	public class TrelloChecklist : IComparable<TrelloChecklist>
	{
		/// <summary>
		/// Id of the board that this checklist is associated to
		/// </summary>
		[JsonProperty("idBoard")]
		public string BoardId { get; set; }

		/// <summary>
		/// Id of the card that this checklist is associated to
		/// </summary>
		[JsonProperty("idCard")]
		public string CardId { get; set; }

		/// <summary>
		/// Items in the checklist
		/// </summary>
		[JsonProperty("checkItems")]
		public IList<TrelloChecklistItem> Items { get; set; }

		/// <summary>
		/// Name of the checklist
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// Sortorder of the checklist
		/// </summary>
		[JsonProperty("pos")]
		public int Pos { get; set; }

		public int CompareTo(TrelloChecklist other)
		{
			return Pos.CompareTo(other.Pos);
		}
	}

	/// <summary>
	/// Checklist item on a checklist
	/// </summary>
	[DebuggerDisplay("{Name} - {IsComplete}")]
	public class TrelloChecklistItem : IComparable<TrelloCard>
	{
		/// <summary>
		/// Name of the checklist item
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// true if State == "complete", false otherwise
		/// </summary>
		public bool IsComplete => string.CompareOrdinal(State, "complete") == 0;

		/// <summary>
		/// Sortorder of the checklist item
		/// </summary>
		[JsonProperty("pos")]
		public int Pos { get; set; }

		/// <summary>
		/// complete | incomplete
		/// </summary>
		[JsonProperty("state")]
		public string State { get; set; }

		public int CompareTo(TrelloCard other)
		{
			return Pos.CompareTo(other.Pos);
		}
		
	}
}