using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloApi;
using TrelloApi.Exceptions;

namespace TrelloTestConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			Trello t = GetTrello();
			if (t == null)
				return;

			var me = t.GetMe();
			var myBoards = t.GetBoards(me);
			var myCards = t.GetCards(me);

			var myCardsWithLabels = myCards.Where(x => x.Labels.Any()).ToList();
		}

		private static Trello GetTrello()
		{
			string token = Trello.GetLastToken();
			int retryCounter = 1;
			while (true)
			{
				try
				{
					var t = new Trello(token);

					// Simply access something that needed 
					t.GetMe();

					return t;
				}
				catch (NoAccessException)
				{
					if (retryCounter-- < 1)
						return null;
					Process.Start(Trello.GetTokenUri("TrelloTestConsole").AbsoluteUri);
					Console.WriteLine("Access denied, enter new token:");
					token = Console.ReadLine();
					if (string.IsNullOrWhiteSpace(token))
						return null;
					Trello.SaveToken(token);
				}
			}
		}
	}
}
