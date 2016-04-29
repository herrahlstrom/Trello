using System.Windows;

namespace TrelloWindow
{
	/// <summary>
	/// Interaction logic for GetTokenWindow.xaml
	/// </summary>
	public partial class GetTokenWindow : Window
	{
		public GetTokenWindow()
		{
			InitializeComponent();

			Browser.Source = TrelloApi.Trello.GetTokenUri("Trello Window");

			DataContext = this;
		}

		public string Token { get; set; }

		private void CloseButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void OkButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			DialogResult = (Token.Length > 5);
		}
	}
}
