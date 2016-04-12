using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
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
		private Trello Trello { get; }

		public MainWindow()
		{
			InitializeComponent();

			if ((Trello = GetTrello()) == null)
			{
				Close();
				return;
			}

			Model = new MainModel();
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
#if DEBUG
					var opts = new TrelloOptions { Token = token, CacheTime = TimeSpan.FromHours(6), PersistentCache = true };
#else
					var opts = new TrelloOptions { Token = token, CacheTime = TimeSpan.FromMinutes(15), PersistentCache = true };
#endif

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
			var me = await Task.Run(() => Trello.Me);

			Model.Members.Add(me);

			var boards = await Task.Run(() => Trello.GetBoards(me));

			foreach (var board in boards.Where(x => !x.IsClosed).OrderByDescending(x => x.IsStarred).ThenBy(x => x.Name))
			{
				Model.Boards.Add(board);

				var boardMembers = await Task.Run(() => Trello.GetMembers(board));
				foreach (var boardMember in boardMembers.OrderBy(x => x.Name))
				{
					if (Model.Members.All(x => x.Id != boardMember.Id))
					{
						Model.Members.Add(boardMember);
					}
				}
			}

			// TEST
			var myCards = Trello.GetCards(me);
			var compactModel = (from c in Trello.GetCards(me)
								let b = Trello.GetBoard(c.BoardId)
								let bl = Trello.GetList(c.ListId)
								let bm = Trello.GetMembers(b)
								orderby b.Name, bl.Pos, c.Pos
								select new PrintCompactModels.Card()
								{
									Name = c.Name,
									Description = c.Description,
									DueDate = c.DueDate?.ToString("yyyy-MM-dd") ?? "",
									Board = new PrintCompactModels.Board() { Name = b.Name },
									List = new PrintCompactModels.BoardList() { Name = bl.Name },
									Members = (from x in bm
											   where c.MemberIds.Contains(x.Id)
											   select new PrintCompactModels.Member()
											   {
												   Name = x.Name,
												   AvatarHash = x.AvatarHash,
												   Username = x.Username,
												   Initials = x.Initials
											   }).ToList(),
									Labels = c.Labels.Select(x => new PrintCompactModels.Label() { Name = x.Name }).ToList()
								}).ToList();

			foreach (var c in Trello.GetCards(me).Take(3))
			{
				var p = GetCardElement(c);
			}

			await Task.Run(() => Trello.SavePersistentCache());
		}

		private void CompactListButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			var m = Model.Members.Where(x => Model.SelectedMemberIds.Contains(x.Id)).ToList();
			var b = Model.Boards.Where(x => Model.SelectedBoardIds.Contains(x.Id)).ToList();
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

		private readonly IDictionary<string, BitmapImage> _avatarCache = new Dictionary<string, BitmapImage>();
		private FrameworkElement GetCardElement(TrelloCard c)
		{
			var board = Trello.GetBoard(c.BoardId);
			var members = (from bm in Trello.GetMembers(board)
						   where c.MemberIds.Contains(bm.Id)
						   orderby bm.Name
						   select bm).ToList();

			var memberPanel = new StackPanel() { MinWidth = 90, Orientation = Orientation.Horizontal };
			foreach (var member in members)
			{
				if (string.IsNullOrWhiteSpace(member.Avatar30Px))
				{
					var b = new Border() { };
					b.Child = new TextBlock() { Text = member.Initials, FontSize = 12, VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Center };
					memberPanel.Children.Add(b);
				}
				else
				{
					BitmapImage bitmap;
					if (!_avatarCache.TryGetValue(member.Avatar30Px, out bitmap))
					{
						bitmap = new BitmapImage();
						bitmap.BeginInit();
						bitmap.UriSource = new Uri(member.Avatar30Px, UriKind.Absolute);
						bitmap.EndInit();
						_avatarCache.Add(member.Avatar30Px, bitmap);
					}
					memberPanel.Children.Add(new Image() { Source = bitmap });
				}
			}

			// todo

			return memberPanel;
		}
	}

	public class MainModel : INotifyPropertyChanged
	{
		public HashSet<string> SelectedMemberIds { get; } = new HashSet<string>();
		public HashSet<string> SelectedBoardIds { get; } = new HashSet<string>();
		public ObservableCollection<TrelloMember> Members { get; } = new ObservableCollection<TrelloMember>();
		public ObservableCollection<TrelloBoard> Boards { get; } = new ObservableCollection<TrelloBoard>();

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}

}