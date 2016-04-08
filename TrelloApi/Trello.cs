using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using Microsoft.Win32;
using Newtonsoft.Json;
using TrelloApi.Exceptions;

namespace TrelloApi
{
	public class Trello
	{
		internal const string ApplicationKey = "b15f1715df3edb45f53d369c36cbbfb2";
		private const string UrlBase = "https://api.trello.com/1/";
		private readonly TrelloOptions _opts;
		private TrelloMember _me;

		public Trello(TrelloOptions opts)
		{
			if (string.IsNullOrWhiteSpace(opts.Token))
				throw new NoAccessException("Missing Token!");

			_opts = opts;
		}

		public TrelloMember Me => _me ?? (_me = GetMember("me"));

		public static string GetLastToken()
		{
			using (var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TrelloApi\Tokens", false))
			{
				return regKey?.GetValue(ApplicationKey, "") as string;
			}
		}

		public static Uri GetTokenUri(string applicationName)
		{
			return new Uri($"https://trello.com/1/connect?expiration=30days&key={ApplicationKey}&name={applicationName}&response_type=token");
		}

		public static void SaveToken(string token)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			using (var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TrelloApi\Tokens", true))
			{
				if (regKey != null)
				{
					regKey.SetValue(ApplicationKey, token);
					return;
				}
			}
			using (var newRk = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\TrelloApi\Tokens", RegistryKeyPermissionCheck.ReadWriteSubTree))
			{
				newRk?.SetValue(ApplicationKey, token);
			}
		}

		public IList<TrelloBoard> GetBoards(TrelloMember member)
		{
			return JsonConvert.DeserializeObject<IList<TrelloBoard>>(SendRequest($"members/{member.Id}/boards/all", "fields=closed,idOrganization,name,starred"));
		}

		public IList<TrelloCard> GetCards(TrelloBoard board)
		{
			return JsonConvert.DeserializeObject<IList<TrelloCard>>(SendRequest($"board/{board.Id}/cards", "fields=closed,desc,due,email,idBoard,idChecklists,idList,idMembers,labels,name,pos,url"));
		}

		public IList<TrelloCard> GetCards(TrelloMember member)
		{
			return JsonConvert.DeserializeObject<IList<TrelloCard>>(SendRequest($"members/{member.Id}/cards", "fields=id,closed,desc,due,email,idBoard,idChecklists,idList,idMembers,labels,name,pos,url"));
		}

		public IList<TrelloChecklist> GetChecklists(TrelloCard card)
		{
			return JsonConvert.DeserializeObject<IList<TrelloChecklist>>(SendRequest($"card/{card.Id}/checklists", "fields=idBoard,idCard,name,pos", "checkItem_fields=name,pos,state"));
		}

		public IList<TrelloLabel> GetLabels(TrelloBoard board)
		{
			return JsonConvert.DeserializeObject<IList<TrelloLabel>>(SendRequest($"board/{board.Id}/labels", "fields=id,color,idBoard,name"));
		}

		public IList<BoardList> GetLists(TrelloBoard board)
		{
			return JsonConvert.DeserializeObject<IList<BoardList>>(SendRequest($"board/{board.Id}/lists", "fields=closed,idBoard,name,pos"));
		}

		public TrelloMember GetMember(string memberId)
		{
			return JsonConvert.DeserializeObject<TrelloMember>(SendRequest($"members/{memberId}", "fields=avatarHash,initials,fullName,username,url"));
		}

		public IList<TrelloMember> GetMembers(TrelloBoard board)
		{
			return JsonConvert.DeserializeObject<IList<TrelloMember>>(SendRequest($"board/{board.Id}/members", "fields=avatarHash,initials,fullName,username,url"));
		}

		private string SendRequest(string path, params string[] parameters)
		{
			string url = $"{UrlBase}{path}?key={ApplicationKey}&token={_opts.Token}";
			if (parameters != null && parameters.Any())
				url += "&" + string.Join("&", parameters);

			// Read from cache
			string cacheKey = CalculateHash(url).ToString();
			if (_opts.CacheTime.TotalSeconds > 0)
			{
				string cacheValue;
				if (_opts.PersistentCache)
					cacheValue = PersistentCache.GetValue(cacheKey);
				else
					cacheValue = MemoryCache.Default.Get(cacheKey) as string;
				if (!string.IsNullOrWhiteSpace(cacheValue))
					return cacheValue;
			}

			using (var wc = new WebClient { Encoding = Encoding.UTF8 })
			{
				try
				{
					string resp = wc.DownloadString(url);

					if (_opts.PersistentCache && _opts.CacheTime.TotalSeconds > 0)
						PersistentCache.SetValue(cacheKey, resp, DateTime.Now.Add(_opts.CacheTime), false);
					else if (_opts.CacheTime.TotalSeconds > 0)
						MemoryCache.Default.Set(cacheKey, resp, DateTimeOffset.Now.Add(_opts.CacheTime));

					return resp;
				}
				catch (WebException ex) when (ex.Message.Contains("(401)"))
				{
					throw new NoAccessException("Access denied!");
				}
			}
		}

		#region Persistens cache

		private DiskStringCache _persistentCache;
		internal DiskStringCache PersistentCache => _persistentCache ?? (_persistentCache = new DiskStringCache(Path.Combine(Path.GetTempPath(), "TrelloCache.cache")));

		public void SavePersistentCache()
		{
			_persistentCache?.Save();
		}

		private static ulong CalculateHash(string str)
		{
			ulong hashedValue = 3074457345618258791ul;
			foreach (char t in str)
			{
				hashedValue += t;
				hashedValue *= 3074457345618258799ul;
			}
			return hashedValue;
		}

		#endregion
	}
}