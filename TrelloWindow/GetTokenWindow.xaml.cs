using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
