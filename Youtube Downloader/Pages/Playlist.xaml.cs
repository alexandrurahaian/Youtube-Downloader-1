using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
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
using Youtube_Downloader.Objects;
using Youtube_Downloader.Others;
using Youtube_Downloader.Scripts;

namespace Youtube_Downloader.Pages
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    public partial class Playlist : Page
    {
        public ObservableCollection<VideoObject> PlaylistVideos { get; set; } = new ObservableCollection<VideoObject>();

        private static readonly string[] AcceptedQuality = ["best", "worst"]; // quality yt-dlp supports

        private static string selected_file_format = ValidFormats.VALID_VIDEO_FORMATS[0];
        private static string selected_quality = AcceptedQuality[0];
        private static string selected_download_location = "C:\\Downloads";
        private static DownloadType selected_download_type = DownloadType.SingleVideo;
        private static DownloadOptions selected_download_option = DownloadOptions.AudioVideo;

        private static int downloaded = 0;
        private static int selected_count = 0;
        private static string playlist_url = string.Empty;

        private static bool isPlaylistLoaded = false;
        public bool UpdateProgress = false;

        public Playlist()
        {
            InitializeComponent();
            this.DataContext = this;
            foreach (string q in AcceptedQuality) quality_combo.Items.Add(q);
            foreach (string q in ValidFormats.VALID_VIDEO_FORMATS) file_format_combo.Items.Add(q);
            download_loc_path_box.Text = selected_download_location;
        }
        private List<VideoObject> GetSelectedVideos()
        {
            List<VideoObject> found_selected = new List<VideoObject>();

            foreach (VideoObject video in PlaylistVideos)
            {
                if (video.Selected == '✅')
                    found_selected.Add(video);
            }

            return found_selected;
        }
        private string[] GetUrls()
        {
            List<string> found_urls = new List<string>();
            foreach (VideoObject video in GetSelectedVideos())
            {
                found_urls.Add(video.VideoURL);
            }
            return found_urls.ToArray();
        }

        public void DisplayProgress(double percent, string size, string speed, string eta, int currentFragment, int totalFragments, bool show_success = false)
        {
            if (!UpdateProgress)
            {
                progress_panel.Dispatcher.Invoke(() =>
                {
                    progress_panel.Visibility = Visibility.Collapsed;
                    download_btn.IsEnabled = true;
                });
                return;
            }
            Debug.WriteLine("In progress");
            progress_panel.Dispatcher.Invoke(() =>
            {
                progress_panel.Visibility = Visibility.Visible;
                download_progress_bar.Value = percent;
                progress_details_display.Text = $"Downloading... {percent}% ~ {size} @ {speed} | {eta} left | fragment {currentFragment} of {totalFragments}";
                download_btn.IsEnabled = false;
                Debug.WriteLine("Set display");
            });
            if (currentFragment >= totalFragments || show_success == true)
            {
                downloaded++;
                if (downloaded < GetSelectedVideos().Count)
                {
                    Debug.WriteLine($"Downloaded: {downloaded}");
                    Debug.WriteLine($"Selected: {GetSelectedVideos().Count}");
                    progress_details_display.Dispatcher.Invoke(() =>
                    {
                        progress_details_display.Text = "Starting next download...";
                        download_progress_bar.Value = 0;
                    });
                }

                if (downloaded >= GetSelectedVideos().Count || show_success == true)
                {
                    UpdateProgress = false;
                    downloaded = 0;
                    progress_panel.Dispatcher.Invoke(() =>
                    {
                        progress_panel.Visibility = Visibility.Collapsed;
                        download_finish_panel.Visibility = Visibility.Visible;
                        download_btn.IsEnabled = true;
                    });
                }
            }
        }

        private void download_btn_Click(object sender, RoutedEventArgs e)
        {
            downloaded = 0;
            download_error_display.Visibility = Visibility.Collapsed;
            download_finish_panel.Visibility = Visibility.Collapsed;

            string[] urls = (selected_count == PlaylistVideos.Count && !string.IsNullOrEmpty(playlist_url) ? [playlist_url] : GetUrls());

            if (urls.Length == 0)
            {
                download_error_display.Visibility = Visibility.Visible;
                MessageBox.Show($"No video URLs found to download!");
                return;
            }
            download_btn.IsEnabled = false;
            Downloader downloader = new Downloader(selected_download_type, selected_file_format, selected_quality, selected_download_location, selected_download_option, urls);
            (bool is_sucess, string? error_msg) = downloader.Download(true);
            if (!is_sucess)
            {
                download_error_display.Visibility = Visibility.Visible;
                download_btn.IsEnabled = true;
                MessageBox.Show($"Something went wrong while attempting to download video(s):{error_msg}");
            }
            else progress_panel.Visibility = Visibility.Visible;
        }

        private void select_all_btn_Click(object sender, RoutedEventArgs e)
        {
            foreach (VideoObject videoObj in PlaylistVideos)
            {
                videoObj.Selected = '✅';
            }
            playlist_content_list.Items.Refresh();
            next_btn.IsEnabled = true;
            selected_count = PlaylistVideos.Count;
        }

        private void deselect_all_btn_Click(object sender, RoutedEventArgs e)
        {
            foreach (VideoObject videoObj in PlaylistVideos)
            {
                videoObj.Selected = 'X';
            }
            playlist_content_list.Items.Refresh();
            next_btn.IsEnabled = false;
            selected_count = 0;
        }

        private static bool loadingPlaylistContents = false;
        private async void load_playlist_video_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!loadingPlaylistContents)
            {
                loadingPlaylistContents = true;
                load_playlist_video_btn.Content = "Loading contents...";
                List<VideoObject> fetched_videos = await DlpHandler.GetPlaylistVideos(playlist_url_box.Text);
                PlaylistVideos.Clear();
                foreach (var video in fetched_videos)
                    PlaylistVideos.Add(video);
                Debug.WriteLine($"Received: {PlaylistVideos.Count}");
                load_playlist_video_btn.Content = "Load playlist";
                load_playlist_video_btn.IsEnabled = false;
                isPlaylistLoaded = true;
                loadingPlaylistContents = false;
                unload_playlist_video_btn.IsEnabled = true;
                next_btn.IsEnabled = true;
                playlist_url = playlist_url_box.Text;
            }
        }
        private void unload_playlist_video_btn_Click(object sender, RoutedEventArgs e)
        {
            PlaylistVideos.Clear();
            downloaded = 0;
            isPlaylistLoaded = false;
            load_playlist_video_btn.IsEnabled = true;
            unload_playlist_video_btn.IsEnabled = false;
            playlist_url = string.Empty;
            next_btn.IsEnabled = false;
            selected_count = 0;
        }

        private void playlist_url_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            load_playlist_video_btn.IsEnabled = (!string.IsNullOrEmpty(playlist_url_box.Text) && Downloader.IsValidURL(playlist_url_box.Text));
        }

        private void search_btn_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(PlaylistVideos);
            if (!string.IsNullOrEmpty(search_box.Text))
            {
                view.Filter = item =>
                {
                    var v = (VideoObject)item;
                    return v.VideoName.Contains(search_box.Text, StringComparison.OrdinalIgnoreCase)
                        || v.VideoURL.Contains(search_box.Text, StringComparison.OrdinalIgnoreCase);
                };
            }
            else view.Filter = null;
        }

        private static bool prev_mode = false;
        private void next_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!prev_mode)
            {
                first_grid.Visibility = Visibility.Hidden;
                options_grid.Visibility = Visibility.Visible;
                prev_mode = true;
                next_btn.Content = "Previous";
            }
            else
            {
                first_grid.Visibility = Visibility.Visible;
                options_grid.Visibility = Visibility.Hidden;
                prev_mode = false;
                next_btn.Content = "Next";
            }
        }

        private void file_format_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (file_format_combo.Items.Count == 0) return;

            if (file_format_combo.SelectedIndex < 0) file_format_combo.SelectedIndex = 0;
            Debug.WriteLine($"Selected Index: {file_format_combo.SelectedIndex}");
            if ((video_audio_radio.IsChecked == true || video_only_radio.IsChecked == true) && file_format_combo.SelectedIndex <= ValidFormats.VALID_VIDEO_FORMATS.Length)
                selected_file_format = ValidFormats.VALID_VIDEO_FORMATS[file_format_combo.SelectedIndex];
            else if (audio_only_radio.IsChecked == true && file_format_combo.SelectedIndex <= ValidFormats.VALID_AUDIO_FORMATS.Length) selected_file_format = ValidFormats.VALID_AUDIO_FORMATS[file_format_combo.SelectedIndex];
        }

        private void quality_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (quality_combo.SelectedIndex < 0) quality_combo.SelectedIndex = 0;
            selected_quality = AcceptedQuality[quality_combo.SelectedIndex];
        }

        private void audio_only_radio_Click(object sender, RoutedEventArgs e)
        {
            selected_download_option = DownloadOptions.AudioOnly;
            file_format_combo.Items.Clear();
            foreach (string q in ValidFormats.VALID_AUDIO_FORMATS) file_format_combo.Items.Add(q);
            file_format_combo.SelectedIndex = 0;
            selected_file_format = ValidFormats.VALID_AUDIO_FORMATS[0];
            file_format_combo.Text = selected_file_format;
            file_format_combo.SelectedIndex = 0;
        }

        private void video_only_radio_Click(object sender, RoutedEventArgs e)
        {
            selected_download_option = DownloadOptions.VideoOnly;
            file_format_combo.Items.Clear();
            foreach (string q in ValidFormats.VALID_VIDEO_FORMATS) file_format_combo.Items.Add(q);
            selected_file_format = ValidFormats.VALID_VIDEO_FORMATS[0];
            file_format_combo.Text = selected_file_format;
            file_format_combo.SelectedIndex = 0;
        }

        private void video_audio_radio_Click(object sender, RoutedEventArgs e)
        {
            selected_download_option = DownloadOptions.AudioVideo;
            file_format_combo.Items.Clear();
            foreach (string q in ValidFormats.VALID_VIDEO_FORMATS) file_format_combo.Items.Add(q);
            selected_file_format = ValidFormats.VALID_VIDEO_FORMATS[0];
            file_format_combo.Text = selected_file_format;
            file_format_combo.SelectedIndex = 0;
        }
        private void download_loc_selector_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog ofd = new OpenFolderDialog();
            ofd.Title = "Select a folder to download video(s) to";
            ofd.Multiselect = false;
            ofd.DefaultDirectory = "C:\\Downloads";
            if (ofd.ShowDialog() == true)
                selected_download_location = ofd.FolderName;
            else if (string.IsNullOrEmpty(selected_download_location)) selected_download_location = ofd.DefaultDirectory;

            download_loc_path_box.Text = ofd.FolderName;
        }

        private void download_loc_path_box_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(download_loc_path_box.Text) && Directory.Exists(download_loc_path_box.Text))
                selected_download_location = download_loc_path_box.Text;
        }

        private void view_finished_download_btn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = selected_download_location + System.IO.Path.DirectorySeparatorChar,
                UseShellExecute = true,
                Verb = "open"
            });
            download_finish_panel.Visibility = Visibility.Collapsed;
        }

        private void deselect_btn_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndx = playlist_content_list.SelectedIndex;
            if (selectedIndx >= 0)
            {
                PlaylistVideos[selectedIndx].Selected = 'X';
            }
            selected_count = Math.Clamp(selected_count - 1, 0, int.MaxValue);
            next_btn.IsEnabled = selected_count > 0;
            playlist_content_list.Items.Refresh();
        }

        private void select_btn_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndx = playlist_content_list.SelectedIndex;
            if (selectedIndx >= 0)
            {
                PlaylistVideos[selectedIndx].Selected = '✅';
            }
            next_btn.IsEnabled = true;
            selected_count = Math.Clamp(selected_count + 1, 0, PlaylistVideos.Count);
            playlist_content_list.Items.Refresh();
        }
    }
}
