using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
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
					var t = new Trello(new TrelloOptions { Token = token });

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
		}

		private bool AnyCrossMatch(IEnumerable<IComparable> a, IEnumerable<IComparable> b)
		{
			var aList = a as IList<IComparable> ?? a.ToList();
			var bList = b as IList<IComparable> ?? b.ToList();

			return aList.Any(aItem => bList.Any(x => x.CompareTo(aItem) == 0));
		}
		private void CompactListButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			var members = Model.Members.Where(x => Model.SelectedMemberIds.Contains(x.Id)).ToList();
			var boards = Model.Boards.Where(x => Model.SelectedBoardIds.Contains(x.Id)).ToList();
			var printCards = new List<TrelloCard>();

			if (boards.Any() && members.Any())
			{
				foreach (var board in boards.OrderBy(x => x.Name))
				{
					// Only get the cards with selected members assignet to it
					printCards.AddRange(
						from c in Trello.GetCards(board)
						let bl = Trello.GetList(c.ListId)
						where AnyCrossMatch(c.MemberIds, members.Select(x => x.Id))
						where printCards.All(x => x.Id != c.Id)
						orderby bl.Pos, c.Pos
						select c
					);
				}
			}
			else if (boards.Any())
			{
				foreach (var board in boards.OrderBy(x => x.Name))
				{
					printCards.AddRange(
						from c in Trello.GetCards(board)
						let bl = Trello.GetList(c.ListId)
						where printCards.All(x => x.Id != c.Id)
						orderby bl.Pos, c.Pos
						select c
					);
				}
			}
			else if (members.Any())
			{
				foreach (var member in members)
				{
					printCards.AddRange(
						from c in Trello.GetCards(member)
						let b = Trello.GetBoard(c.BoardId)
						let bl = Trello.GetList(c.ListId)
						where printCards.All(x => x.Id != c.Id)
						orderby b.Name, bl.Pos, c.Pos
						select c
					);
				}
			}

			if (!printCards.Any())
				return;

			var result = Print(printCards);
			System.Diagnostics.Process.Start(result.FullName);

			//todo:genomför utskrift
		}

		private FileInfo Print(IEnumerable<TrelloCard> cards)
		{
			// Denna siffra matchar innehållet som wpf-kontrollerna är breddanpassade efter, mindre siffra=mer zoom
			const double contentWidth = 700;
			const double contentMaxHeight = 880;

			var doc = new FixedDocument();
			var pagePanel = new StackPanel() { Width = contentWidth };
			int pageHeight = 0;

			TrelloBoard currentBoard = null;
			TrelloBoardList currentList = null;
			foreach (var card in cards)
			{
				if (pageHeight >= contentMaxHeight)
				{
					doc.Pages.Add(CreatePageContent(pagePanel));
					pagePanel = new StackPanel() { Width = contentWidth };
					pageHeight = 0;
				}

				if (currentBoard == null || currentBoard.Id != card.BoardId)
				{
					if (pageHeight >= contentMaxHeight - 100)
					{
						// Tavlor påbörjas inte för nära slutet på sidan
						doc.Pages.Add(CreatePageContent(pagePanel));
						pagePanel = new StackPanel() { Width = contentWidth };
						pageHeight = 0;
					}

					currentBoard = Trello.GetBoard(card.BoardId);
					currentList = null;
					pagePanel.Children.Add(new TextBlock { Text = currentBoard.Name, FontSize = 20, Margin = new Thickness(0, 10, 0, 5) });
					pageHeight += 30;//Schablon
				}
				if (currentList == null || currentList.Id != card.ListId)
				{
					if (pageHeight >= contentMaxHeight - 50)
					{
						// Listor påbörjas inte för nära slutet på sidan
						doc.Pages.Add(CreatePageContent(pagePanel));
						pagePanel = new StackPanel() { Width = contentWidth };
						pageHeight = 0;
					}
					currentList = Trello.GetList(card.ListId);
					pagePanel.Children.Add(new TextBlock { Text = currentList.Name, FontSize = 14, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 5) });
					pageHeight += 30;//Schablon
				}

				pagePanel.Children.Add(GetCardElement(currentBoard, card));
				pageHeight += 30;//Schablon

			}
			if (pagePanel != null && pagePanel.Children.Count > 0)
			{
				doc.Pages.Add(CreatePageContent(pagePanel));
			}


			byte[] xpsData;

			// Skapa ett xps-dokument, och konvertera till en byte-array
			using (var ms = new MemoryStream())
			{
				var package = Package.Open(ms, FileMode.CreateNew, FileAccess.ReadWrite);
				using (var xpsd = new XpsDocument(package, CompressionOption.NotCompressed))
				{
					var xw = XpsDocument.CreateXpsDocumentWriter(xpsd);
					xw.Write(doc);
				}
				xpsData = ms.ToArray();
			}

			string tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".xps");
			File.WriteAllBytes(tmpFile, xpsData);

			return new FileInfo(tmpFile);

		}

		private static PageContent CreatePageContent(StackPanel pagePanel)
		{
			// Denna siffra är empiriskt framtagen efter A4
			const double pageWidth = 815;

			var pageMargin = new Thickness(70, 50, 50, 50);
			var vb = new Viewbox
			{
				Margin = pageMargin,
				Width = pageWidth - (pageMargin.Left + pageMargin.Right),
				Child = pagePanel
			};
			var content = new PageContent();
			var page = new FixedPage();

			// Bygg upp dokumentet
			page.Children.Add(vb);
			((IAddChild)content).AddChild(page);

			return content;

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

		private FrameworkElement GetCardElement(TrelloBoard b, TrelloCard c)
		{
			var members = (from bm in Trello.GetMembers(b)
						   where c.MemberIds.Contains(bm.Id)
						   orderby bm.Name
						   select bm).ToList();
			var panel = new DockPanel();

			var memberPanel = GetCardMembersPanel(members);
			panel.Children.Add(memberPanel);
			DockPanel.SetDock(memberPanel, Dock.Left);

			if (c.DueDate.HasValue)
			{
				var dueDatePanel = new StackPanel() { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };

				if (c.DueDate.Value < DateTime.Now)
				{
					dueDatePanel.Children.Add(new TextBlock() { Text = "Förfallit", FontSize = 10 });
					dueDatePanel.Children.Add(new TextBlock() { Text = c.DueDate.Value.ToString("dd MMM yyyy"), FontSize = 10, Foreground = new SolidColorBrush(Colors.Red), FontWeight = FontWeights.Bold });
				}
				else
				{
					dueDatePanel.Children.Add(new TextBlock() { Text = "Förfaller", FontSize = 10 });
					dueDatePanel.Children.Add(new TextBlock() { Text = c.DueDate.Value.ToString("dd MMM yyyy"), FontSize = 10 });
				}

				panel.Children.Add(dueDatePanel);
				DockPanel.SetDock(dueDatePanel, Dock.Right);
			}
			if (c.Labels.Any())
			{
				var labelPanel = new StackPanel() { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
				foreach (var label in c.Labels)
				{
					var rgbColor = label.GetRgbColor();
					// Set lightly transparent
					rgbColor.A = 128;

					labelPanel.Children.Add(new TextBlock()
					{
						Text = label.Name,
						Background = new SolidColorBrush(rgbColor),
						FontSize = 10,
						Margin = new Thickness(10, 0, 0, 0),
						Padding = new Thickness(5, 0, 5, 0)
					});
				}
				panel.Children.Add(labelPanel);
				DockPanel.SetDock(labelPanel, Dock.Right);
			}

			panel.Children.Add(new TextBlock()
			{
				VerticalAlignment = VerticalAlignment.Center,
				Text = c.Name,
				TextWrapping = TextWrapping.Wrap,
				FontSize = 14
			});

			var wrapper = new Border()
			{
				BorderBrush = new SolidColorBrush(Color.FromRgb(0xee, 0xee, 0xee)),
				BorderThickness = new Thickness(0, 0, 0, 1),
				Child = panel,
				Padding = new Thickness(0, 0, 0, 2),
				Margin = new Thickness(0, 0, 0, 3)
			};
			return wrapper;
		}

		private StackPanel GetCardMembersPanel(List<TrelloMember> members)
		{
			var memberPanel = new StackPanel() { Orientation = Orientation.Horizontal, MinWidth = 90 };
			foreach (var member in members)
			{
				BitmapImage avatarBitmap = GetMemberAvatarBitmap(member);

				if (avatarBitmap == null)
				{
					var b = new Border
					{
						BorderBrush = new SolidColorBrush(Color.FromRgb(0xaa, 0xaa, 0xaa)),
						Background = new SolidColorBrush(Color.FromRgb(0xee, 0xee, 0xee)),
						Width = 24,
						Height = 24,
						CornerRadius = new CornerRadius(16),
						Margin = new Thickness(0, 0, 4, 0),
						Child = new TextBlock()
						{
							Text = member.Initials,
							FontSize = 12,
							VerticalAlignment = VerticalAlignment.Center,
							TextAlignment = TextAlignment.Center
						}
					};
					memberPanel.Children.Add(b);
				}
				else
				{
					var img = new Image()
					{
						Width = 24,
						Height = 24,
						Margin = new Thickness(0, 0, 4, 0),
						Source = avatarBitmap
					};
					memberPanel.Children.Add(img);
					RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
				}
			}

			return memberPanel;
		}

		private static BitmapImage GetMemberAvatarBitmap(TrelloMember member)
		{
			if (string.IsNullOrWhiteSpace(member?.Avatar170Px))
				return null;

			byte[] imgData = MemoryCache.Default.Get("MEMBER_IMG_" + member.Id) as byte[];
			if (imgData == null)
			{
				using (var wc = new System.Net.WebClient())
					imgData = wc.DownloadData(member.Avatar170Px);
				MemoryCache.Default.Set("MEMBER_IMG_" + member.Id, imgData, DateTimeOffset.Now.AddHours(1));
			}

			var bitmap = new BitmapImage();
			using (var mem = new MemoryStream(imgData))
			{
				mem.Position = 0;
				bitmap.BeginInit();
				bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.UriSource = null;
				bitmap.StreamSource = mem;
				bitmap.EndInit();
			}
			bitmap.Freeze();
			return bitmap;
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