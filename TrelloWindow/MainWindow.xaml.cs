using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrelloApi;
using TrelloApi.Exceptions;

namespace TrelloWindow
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainModel _model;

		public MainWindow()
		{
			InitializeComponent();

			var t = GetTrello();

			if (t == null)
			{
				Close();
				return;
			}

			Model = new MainModel(t);
		}

		public MainModel Model
		{
			get { return _model; }
			set
			{
				_model = value;
				DataContext = value;
			}
		}

		private Trello GetTrello()
		{
			string token = Trello.GetLastToken();

			while (true)
			{
				try
				{
					var opts = new TrelloOptions { Token = token, CacheTime = TimeSpan.FromMinutes(30), PersistentCache = true };

					var t = new Trello(opts);

					// Simply access something that needed access, to eventually trigger NoAccessException
					t.GetMember("me");
					return t;
				}
				catch (NoAccessException)
				{
					var getTokenWin = new GetTokenWindow();
					if (getTokenWin.ShowDialog() ?? false)
					{
						Trello.SaveToken(token = getTokenWin.Token);
						continue;
					}
					return null;
				}
			}
		}

		private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			var me = await Task.Run(() => Model.Trello.Me);

			Model.Members.Add(me);

			var boards = await Task.Run(() => Model.Trello.GetBoards(me));

			foreach (var board in boards.Where(x => !x.IsClosed).OrderByDescending(x => x.IsStarred).ThenBy(x => x.Name))
			{
				Model.Boards.Add(board);

				var boardMembers = await Task.Run(() => Model.Trello.GetMembers(board));
				foreach (var boardMember in boardMembers.OrderBy(x => x.Name))
				{
					if (Model.Members.All(x => x.Id != boardMember.Id))
					{
						Model.Members.Add(boardMember);
					}
				}
			}

			await Task.Run(() => Model.Trello.SavePersistentCache());
		}

		private bool AnyCrossMatch(IEnumerable<IComparable> a, IEnumerable<IComparable> b)
		{
			var aList = a as IList<IComparable> ?? a.ToList();
			var bList = b as IList<IComparable> ?? b.ToList();

			foreach (var aItem in aList)
			{
				if (bList.Any(x => x.CompareTo(aItem) == 0))
					return true;
			}
			return false;
		}
		private void CompactListButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			var members = Model.Members.Where(x => Model.SelectedMemberIds.Contains(x.Id)).ToList();
			var boards = Model.Boards.Where(x => Model.SelectedBoardIds.Contains(x.Id)).ToList();
			var printCards = new List<TrelloCard>();

			if (boards.Any() && members.Any())
			{
				foreach (var board in boards)
				{
					// Only get the cards with selected members assignet to it
					printCards.AddRange(
						from c in Model.Trello.GetCards(board)
						where AnyCrossMatch(c.MemberIds, members.Select(x => x.Id))
						where !printCards.Any(x=> x.Id == c.Id) 
						select c
					);
				}
			}
			else if (boards.Any())
			{
				foreach (var board in boards)
				{
					printCards.AddRange(
						from c in Model.Trello.GetCards(board)
						where !printCards.Any(x => x.Id == c.Id)
						select c
					);
				}
			}
			else if (members.Any())
			{
				foreach (var member in members)
				{
					printCards.AddRange(
						from c in Model.Trello.GetCards(member)
						where !printCards.Any(x => x.Id == c.Id)
						select c
					);
				}
			}

			if (!printCards.Any())
				return;
		}

		private void MemberSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems != null)
			{
				foreach (TrelloMember item in e.AddedItems)
					Model.SelectedMemberIds.Add(item.Id);
			}
			if (e.RemovedItems != null)
			{
				foreach (TrelloMember item in e.RemovedItems)
					Model.SelectedMemberIds.Remove(item.Id);
			}
		}
		private void BoardSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems != null)
			{
				foreach (TrelloBoard item in e.AddedItems)
					Model.SelectedBoardIds.Add(item.Id);
			}
			if (e.RemovedItems != null)
			{
				foreach (TrelloBoard item in e.RemovedItems)
					Model.SelectedBoardIds.Remove(item.Id);
			}
		}

	}

	public class MainModel : INotifyPropertyChanged
	{
		public MainModel(Trello trello)
		{
			Trello = trello;
		}

		public HashSet<string> SelectedMemberIds { get; } = new HashSet<string>();
		public HashSet<string> SelectedBoardIds { get; } = new HashSet<string>();
		public ObservableCollection<TrelloMember> Members { get; } = new ObservableCollection<TrelloMember>();
		public ObservableCollection<TrelloBoard> Boards { get; } = new ObservableCollection<TrelloBoard>();

		public Trello Trello { get; }

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}

}