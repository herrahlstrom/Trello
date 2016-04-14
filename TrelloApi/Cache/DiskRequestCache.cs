using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TrelloApi.Cache
{
	internal class DiskRequestCache : IRequestCache
	{
		private readonly TimeSpan _cacheTime;

		public DiskRequestCache(TimeSpan cacheTime)
		{
			_cacheTime = cacheTime;
		}

		private string GetFilePath(string key)
		{
			using (MD5 md5Hash = MD5.Create())
			{
				// Convert the input string to a byte array and compute the hash.
				byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(key));

				// Create a new Stringbuilder to collect the bytes
				// and create a string.
				StringBuilder sBuilder = new StringBuilder();

				// Loop through each byte of the hashed data 
				// and format each one as a hexadecimal string.
				foreach (byte t in data)
				{
					sBuilder.Append(t.ToString("x2"));
				}

				// Return the hexadecimal string.
				return Path.Combine(Path.GetTempPath(), "TrelloApiCache", $"{sBuilder}.txt");
			}
		}

		public string GetValue(string key)
		{
			var cFile = new FileInfo(GetFilePath(key));
			if (!cFile.Exists)
				return null;
			if ((DateTime.Now - cFile.LastWriteTime) > _cacheTime)
			{
				cFile.Delete();
				return null;
			}
			using (var reader = new StreamReader(cFile.FullName, Encoding.UTF8))
				return reader.ReadToEnd();
		}

		public void SetValue(string key, string value)
		{
			var cFile = new FileInfo(GetFilePath(key));
			if(cFile.Directory != null && !cFile.Directory.Exists)
				cFile.Directory.Create();
			using (var writer = new StreamWriter(cFile.FullName, false, Encoding.UTF8))
				writer.Write(value);
		}
	}
}