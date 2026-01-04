using System.Configuration;
using System.Data;
using System.Windows;

namespace Youtube_Downloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void ChangeTheme(string theme)
        {
            Application currentApp = Application.Current;

            var dictionaries = currentApp.Resources.MergedDictionaries;

            for (int i = dictionaries.Count - 1; i >= 0; i--)
            {
                var source = dictionaries[i].Source.ToString();
                if (!string.IsNullOrEmpty(source) && source.Contains("Theme"))
                    dictionaries.RemoveAt(i);
            }

            dictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Themes/{theme}.xaml", UriKind.Relative)
            });
        }
    }

}
