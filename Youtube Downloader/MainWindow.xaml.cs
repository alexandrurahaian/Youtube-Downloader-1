using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Youtube_Downloader.Pages;
using Youtube_Downloader.Scripts;

namespace Youtube_Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Video video_page = new Video();
        private static Playlist playlist_page = new Playlist();
        private static int theme = 0;
        public MainWindow()
        {
            InitializeComponent();
            mode_display.Content = video_page;
            DlpHandler.video_page = video_page;
            DlpHandler.playlist_page = playlist_page;
        }

        private void playlist_radio_btn_Click(object sender, RoutedEventArgs e)
        {
            mode_display.Content = playlist_page;
        }

        private void video_radio_btn_Click(object sender, RoutedEventArgs e)
        {
            mode_display.Content = video_page;
        }

        private void theme_btn_Click(object sender, RoutedEventArgs e)
        {
            if (theme == 0)
            {
                theme = 1;
                App.ChangeTheme("Dark");
                theme_btn.Content = new Image
                {
                    Source = new BitmapImage(new Uri($"/Assets/sun.png", UriKind.RelativeOrAbsolute))
                };
            }
            else
            {
                theme = 0;
                App.ChangeTheme("Light");
                theme_btn.Content = new Image
                {
                    Source = new BitmapImage(new Uri($"/Assets/moon.png", UriKind.RelativeOrAbsolute))
                };
            }
        }
    }
}