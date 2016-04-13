using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace TrelloApi
{
	/// <summary>
	///     Label that can be associated to a card
	/// </summary>
	[DebuggerDisplay("{Name}, {Color}")]
	public class TrelloLabel : IComparable<TrelloLabel>
	{
		/// <summary>
		///     Id of the board that this label is associated to
		/// </summary>
		[JsonProperty("idBoard")]
		public string BoardId { get; set; }

		/// <summary>
		///     Colorname in text
		/// </summary>
		[JsonProperty("color")]
		public string Color { get; set; }

		/// <summary>
		///     Color that can be used in html-pages
		/// </summary>
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
		public Tuple<byte, byte, byte> RgbColor
		{
			get
			{
				//ToDo: lime
				switch (Color)
				{
					case "red":
						return Tuple.Create((byte)0x61, (byte)0xbd, (byte)0x4f);
					case "green":
						return Tuple.Create((byte)0x61, (byte)0xbd, (byte)0x4f);
					case "blue":
						return Tuple.Create((byte)0x00, (byte)0x79, (byte)0xbf);
					case "purple":
						return Tuple.Create((byte)0xc3, (byte)0x77, (byte)0xe0);
					case "orange":
						return Tuple.Create((byte)0xff, (byte)0xab, (byte)0x4a);
					case "yellow":
						return Tuple.Create((byte)0xf2, (byte)0xd6, (byte)0x00);
					default:
						return null;
				}
			}
		}

		/// <summary>
		///     Label id
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; }

		/// <summary>
		///     Labeltext
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		public int CompareTo(TrelloLabel other)
		{
			return string.Compare(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}