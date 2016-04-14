using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Win32;
using Newtonsoft.Json;
using TrelloApi.Cache;
using TrelloApi.Exceptions;

namespace TrelloApi
{
	public class Trello
	{
		internal const string ApplicationKey = "b15f1715df3edb45f53d369c36cbbfb2";
		private const string UrlBase = "https://api.trello.com/1/";

		private const string BoardFields = "closed,idOrganization,name,starred";
		private const string MemberFields = "id,avatarHash,initials,fullName,username,url";
		private const string ListFields = "closed,id,idBoard,name,pos";
		private const string CardFields = "closed,desc,due,email,idBoard,idChecklists,idList,idMembers,labels,name,pos,url";
		private const string ChecklistFields = "idBoard,idCard,name,pos";
		private const string ChecklistItemFields = "name,pos,state";
		private const string LabelFields = "id,color,idBoard,name";
		private readonly TrelloOptions _opts;
		private TrelloMember _me;

		private readonly IRequestCache _memCache = new MemoryRequestCache(TimeSpan.FromMinutes(1));
#if DEBUG
		private readonly IRequestCache _dskCache = new DiskRequestCache(TimeSpan.FromMinutes(120));
#else
		private readonly IRequestCache _dskCache = null;
#endif

		public Trello(TrelloOptions opts)
		{
			if (string.IsNullOrWhiteSpace(opts?.Token))
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

		public TrelloBoard GetBoard(string id)
		{
			return JsonConvert.DeserializeObject<TrelloBoard>(SendRequest($"board/{id}", "fields=" + BoardFields));
		}

		public IList<TrelloBoard> GetBoards(TrelloMember member)
		{
			return JsonConvert.DeserializeObject<IList<TrelloBoard>>(SendRequest($"members/{member.Id}/boards/all", "fields=" + BoardFields));
		}

		public TrelloCardExtended GetCard(string id)
		{
			return JsonConvert.DeserializeObject<TrelloCardExtended>(SendRequest($"card/{id}", "fields=" + CardFields, "list=true", "list_fields=" + ListFields, "board=true", "board_fields=" + BoardFields));
		}

		public IList<TrelloCard> GetCards(TrelloBoard board)
		{
			return JsonConvert.DeserializeObject<IList<TrelloCard>>(SendRequest($"board/{board.Id}/cards", "fields=" + CardFields));
		}

		public IList<TrelloCard> GetCards(TrelloBoardList list)
		{
			return JsonConvert.DeserializeObject<IList<TrelloCard>>(SendRequest($"list/{list.Id}/cards", "fields=" + CardFields));
		}

		public IList<TrelloCard> GetCards(TrelloMember member)
		{
			return JsonConvert.DeserializeObject<IList<TrelloCard>>(SendRequest($"members/{member.Id}/cards", "fields=" + CardFields));
		}

		public IList<TrelloChecklist> GetChecklists(TrelloCard card)
		{
			return JsonConvert.DeserializeObject<IList<TrelloChecklist>>(SendRequest($"card/{card.Id}/checklists", "fields=" + ChecklistFields, "checkItem_fields=" + ChecklistItemFields));
		}

		public IList<TrelloLabel> GetLabels(TrelloBoard board)
		{
			return JsonConvert.DeserializeObject<IList<TrelloLabel>>(SendRequest($"board/{board.Id}/labels", "fields=" + LabelFields));
		}

		public TrelloBoardsListExtended GetList(string id)
		{
			return JsonConvert.DeserializeObject<TrelloBoardsListExtended>(SendRequest($"list/{id}", "fields=" + ListFields, "board=true", "board_fields=" + BoardFields));
		}

		public IList<TrelloBoardList> GetLists(TrelloBoard board)
		{
			return JsonConvert.DeserializeObject<IList<TrelloBoardList>>(SendRequest($"board/{board.Id}/lists", "fields=" + ListFields));
		}

		public TrelloMember GetMember(string memberId)
		{
			return JsonConvert.DeserializeObject<TrelloMember>(SendRequest($"members/{memberId}", "fields=" + MemberFields));
		}

		public IList<TrelloMember> GetMembers(TrelloBoard board)
		{
			return JsonConvert.DeserializeObject<IList<TrelloMember>>(SendRequest($"board/{board.Id}/members", "fields=" + MemberFields));
		}

		private string SendRequest(string path, params string[] parameters)
		{
			string url = $"{UrlBase}{path}?key={ApplicationKey}&token={_opts.Token}";
			if (parameters != null && parameters.Any())
				url += "&" + string.Join("&", parameters);

			// Read from cache
			string resp =
				_memCache?.GetValue(url)
				?? _dskCache?.GetValue(url);

			if (resp == null)
			{
				using (var wc = new WebClient { Encoding = Encoding.UTF8 })
				{
					try
					{
						Debug.WriteLine("Download " + url);
						resp = wc.DownloadString(url);
					}
					catch (WebException ex) when (ex.Message.Contains("(401)"))
					{
						throw new NoAccessException("Access denied!");
					}
				}

				// Save to cache
				_memCache?.SetValue(url, resp);
				_dskCache?.SetValue(url, resp);
			}

			return resp;
		}
		
	}
}