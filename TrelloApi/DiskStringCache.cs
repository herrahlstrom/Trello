using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TrelloApi
{
	internal class DiskStringCache
	{
		private const int Version = 1;
		private readonly IDictionary<string, DiskCacheItem> _items = new Dictionary<string, DiskCacheItem>();
		private readonly string _path;

		public DiskStringCache(string path)
		{
			_path = path;
			if (File.Exists(_path))
			{
				foreach (var item in LoadItems(_path))
					_items.Add(item.Key, item);
			}
		}

		public string GetValue(string key)
		{
			DiskCacheItem item;
			if (_items.TryGetValue(key, out item))
			{
				if (item.TimeToLive >= DateTime.Now)
					return item.Value;
			}
			return null;
		}

		public void Save()
		{
			SaveItems(_path, _items.Values);
		}

		public void SetValue(string key, string value, DateTime ttl, bool autoSave = true)
		{
			DiskCacheItem item;
			if (_items.TryGetValue(key, out item))
			{
				item.Value = value;
				item.TimeToLive = ttl;
			}
			else
			{
				item = new DiskCacheItem
				{
					Key = key,
					Value = value,
					TimeToLive = ttl
				};
				_items.Add(key, item);
			}
			if (autoSave)
				SaveItems(_path, _items.Values);
		}

		private static IEnumerable<char> GetFileChars(string path)
		{
			using (var f = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var ms = new StreamReader(f, Encoding.UTF8))
			{
				var buffer = new char[1024];
				while (true)
				{
					int l = ms.Read(buffer, 0, buffer.Length);
					if (l == 0)
						break;
					for (int i = 0; i < l; i++)
						yield return buffer[i];
				}
			}
		}

		private static IEnumerable<DiskCacheItem> LoadItems(string path)
		{
			var chars = GetFileChars(path).GetEnumerator();

			string version = "0";
			while (chars.MoveNext() && chars.Current != (char)13 && chars.Current != (char)10)
				version += chars.Current;

			if (int.Parse(version) < Version)
				yield break;

			while (chars.MoveNext())
			{
				if (chars.Current == (char)13)
					continue;
				if (chars.Current == (char)10)
					continue;
			
				var item = new DiskCacheItem();

				item.Key += chars.Current;
				while (chars.MoveNext() && chars.Current != '	')
					item.Key += chars.Current;

				string ttl = "";
				while (chars.MoveNext() && chars.Current != '	')
					ttl += chars.Current;
				item.TimeToLive = DateTime.Parse(ttl);

				string length = "";
				while (chars.MoveNext() && chars.Current != '	')
					length += chars.Current;
				int dataLength = int.Parse(length);

				var data = new StringBuilder(dataLength);
				while (data.Length < dataLength)
				{
					if (chars.MoveNext())
						data.Append(chars.Current);
					else
						break;
				}
				item.Value = data.ToString();

				yield return item;
			}
		}

		private static void SaveItems(string path, IEnumerable<DiskCacheItem> items)
		{
			using (var f = File.Open(path, FileMode.Create, FileAccess.Write))
			using (var ms = new StreamWriter(f, Encoding.UTF8))
			{
				ms.Write(Version);
				ms.Write('\n');

				foreach (var item in items.Where(x => x.TimeToLive > DateTime.Now))
				{
					ms.Write(item.Key);
					ms.Write('	');
					ms.Write(item.TimeToLive.ToString("u"));
					ms.Write('	');
					ms.Write(item.Value.Length);
					ms.Write('	');
					ms.Write(item.Value);
					ms.WriteLine();
				}
			}
		}

		private class DiskCacheItem
		{
			public string Key { get; set; } = "";
			public DateTime TimeToLive { get; set; }
			public string Value { get; set; } = "";
		}
	}
}