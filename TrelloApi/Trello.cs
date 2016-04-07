using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using TrelloApi.Exceptions;

namespace TrelloApi
{
	public class Trello
	{
		internal const string ApplicationKey = "b15f1715df3edb45f53d369c36cbbfb2";
		private const string UrlBase = "https://api.trello.com/1/";
		private readonly string _token;

		public Trello(string token)
		{
			_token = token;
		}

		public static string GetLastToken()
		{
			using (var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\TrelloApi\Tokens", false))
			{
				return regKey?.GetValue(ApplicationKey, "") as string;
			}
		}
		public static Uri GetTokenUri(string applicationName)
		{
			return new Uri($"https://trello.com/1/connect?key={Trello.ApplicationKey}&name={applicationName}&response_type=token");
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
				return;
			}
		}

		public TrelloMember GetMe()
		{
			return GetMember("me");
		}
		public TrelloMember GetMember(string memberId)
		{
			return JsonConvert.DeserializeObject<TrelloMember>(SendRequest($"members/{memberId}", "fields=avatarHash,initials,fullName,username"));
		}
		public IList<TrelloBoard> GetBoards(TrelloMember member)
		{
			return JsonConvert.DeserializeObject<IList<TrelloBoard>>(SendRequest($"members/{member.UserId}/boards/all", "fields=closed,idOrganization,name,starred"));
		}
		public IList<TrelloCard> GetCards(TrelloMember member)
		{
			return JsonConvert.DeserializeObject<IList<TrelloCard>>(SendRequest($"members/{member.UserId}/cards", "fields=closed,desc,due,email,idBoard,idChecklists,idList,idMembers,labels,name,pos,url"));
		}

		private string SendRequest(string path, params string[] parameters)
		{
			if (string.IsNullOrWhiteSpace(_token))
				throw new NoAccessException("Missing Token!");

			string url = $"{UrlBase}{path}?key={Trello.ApplicationKey}&token={_token}";
			if (parameters != null && parameters.Any())
				url += "&" + string.Join("&", parameters);

			string cacheKey = url;
			var cacheValue = MemoryCache.Default.Get(cacheKey) as string;
			if (!string.IsNullOrWhiteSpace(cacheValue))
				return cacheValue;

			using (var wc = new WebClient { Encoding = Encoding.UTF8 })
			{
				try
				{
					string resp = wc.DownloadString(url);

					// Cache everything for a short time. This helps surprised worth much
					MemoryCache.Default.Set(cacheKey, resp, DateTimeOffset.Now.AddMinutes(1));

					return resp;
				}
				catch (WebException ex) when (ex.Message.Contains("(401)"))
				{
					throw new NoAccessException("Access denied!");
				}
			}
		}

	}
}
