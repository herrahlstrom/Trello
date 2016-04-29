using System;
using System.Diagnostics;
using System.Windows.Media;
using Newtonsoft.Json;

namespace TrelloApi
{
	/// <summary>
	///     Label that can be associated to a card
	/// </summary>
	[DebuggerDisplay("{Name}, {ColorName}")]
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
		public string ColorName { get; set; }

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

		/// <summary>
		///     Color of the label, represented in #RRGGBB
		/// </summary>
		public string GetHexColor()
		{
			var rgb = GetRgbColor();
			return $"#{rgb.R}{rgb.G}{rgb.B}";
		}

		/// <summary>
		///     Color of the label, represented by rgb tuple
		/// </summary>
		/// <returns></returns>
		public Color GetRgbColor()
		{
			switch (ColorName)
			{
				case "red":
					return Color.FromRgb(0xeb, 0x5a, 0x46);
				case "green":
					return Color.FromRgb(0x61, 0xbd, 0x4f);
				case "blue":
					return Color.FromRgb(0x00, 0x79, 0xbf);
				case "purple":
					return Color.FromRgb(0xc3, 0x77, 0xe0);
				case "orange":
					return Color.FromRgb(0xff, 0xab, 0x4a);
				case "yellow":
					return Color.FromRgb(0xf2, 0xd6, 0x00);
				case "black":
					return Color.FromRgb(0x4d, 0x4d, 0x4d);
				case "pink":
					return Color.FromRgb(0xff, 0x80, 0xce);
				case "lime":
					return Color.FromRgb(0x51, 0xe8, 0x98);
				case "sky":
					return Color.FromRgb(0x00, 0xc2, 0xe0);
				default:
					return Color.FromRgb(0xff, 0xff, 0xff);
			}
		}
	}
}