using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static System.Net.Mime.MediaTypeNames;

namespace Youtube_Downloader.Pages
{
    public partial class Video : Page
    {
        private static readonly string[] AcceptedQuality = ["best", "worst"]; // quality yt-dlp supports

        public ObservableCollection<VideoObject> BatchVideos { get; set; } = new ObservableCollection<VideoObject>();
        private static int batch_index = 0;

        private static string selected_file_format = ValidFormats.VALID_VIDEO_FORMATS[0];
        private static string selected_quality = AcceptedQuality[0];
        private static string selected_download_location = "C:\\Downloads";
        private static DownloadType selected_download_type = DownloadType.SingleVideo;
        private static DownloadOptions selected_download_option = DownloadOptions.AudioVideo;


        private static bool isBatch = false;

        public Video()
        {
            InitializeComponent();
            this.DataContext = this;
            foreach (string q in AcceptedQuality) quality_combo.Items.Add(q);
            foreach (string q in ValidFormats.VALID_VIDEO_FORMATS) file_format_combo.Items.Add(q);
            download_loc_path_box.Text = selected_download_location;
        }

        private string[] GetUrls()
        {
            List<string> found_urls = new List<string>();
            if (isBatch)
            {
                foreach (VideoObject video in GetSelectedVideos())
                {
                    found_urls.Add(video.VideoURL);
                }
            }
            else found_urls.Add(url_box.Text);
            return found_urls.ToArray();
        }

        public bool UpdateProgress = false;
        int downloaded = 0;

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
            progress_panel.Dispatcher.Invoke(() =>
            {
                progress_panel.Visibility = Visibility.Visible;
                download_progress_bar.Value = percent;
                progress_details_display.Text = $"Downloading... {percent}% ~ {size} @ {speed} | {eta} left | fragment {currentFragment} of {totalFragments}";
                download_btn.IsEnabled = false;
            });
            if (currentFragment >= totalFragments)
            {
                downloaded++;
                if (isBatch && downloaded < GetSelectedVideos().Count)
                {
                    Debug.WriteLine($"Downloaded: {downloaded}");
                    Debug.WriteLine($"Selected: {GetSelectedVideos().Count}");
                    progress_details_display.Dispatcher.Invoke(() =>
                    {
                        progress_details_display.Text = "Starting next download...";
                        download_progress_bar.Value = 0;
                    });
                }

                if (!isBatch || downloaded >= GetSelectedVideos().Count || show_success)
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

            string[] urls = GetUrls();

            if (urls.Length == 0)
            {
                download_error_display.Visibility = Visibility.Visible;
                MessageBox.Show($"No video URLs found to download!");
                return;
            }
            download_btn.IsEnabled = false;
            Downloader downloader = new Downloader(selected_download_type, selected_file_format, selected_quality, selected_download_location, selected_download_option, urls);
            (bool is_sucess, string? error_msg) = downloader.Download();
            if (!is_sucess)
            {
                download_error_display.Visibility = Visibility.Visible;
                download_btn.IsEnabled = true;
                MessageBox.Show($"Something went wrong while attempting to download video(s):{error_msg}");
            }
            else progress_panel.Visibility = Visibility.Visible;
        }

        private void single_radio_btn_Click(object sender, RoutedEventArgs e)
        {
            batch_grid.Visibility = Visibility.Collapsed;
            options_grid.Visibility = Visibility.Visible;
            bottom_panel.Visibility = Visibility.Collapsed;
            isBatch = false;
        }

        private void batch_radio_btn_Click(object sender, RoutedEventArgs e)
        {
            batch_grid.Visibility = Visibility.Visible;
            options_grid.Visibility = Visibility.Collapsed;
            bottom_panel.Visibility = Visibility.Visible;
            isBatch = true;
            if (BatchVideos.Count > 0) add_video_btn.IsEnabled = true;
            else add_video_btn.IsEnabled = false;
        }

        private void url_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (batch_radio_btn.IsChecked == true)
                add_video_btn.IsEnabled = (!string.IsNullOrEmpty(url_box.Text) && Downloader.IsValidURL(url_box.Text));
            else
                download_btn.IsEnabled = (!string.IsNullOrEmpty(url_box.Text) && !string.IsNullOrEmpty(download_loc_path_box.Text) && Downloader.IsValidURL(url_box.Text));
        }

        // BATCH PANEL LOGIC

        private List<VideoObject> GetSelectedVideos()
        {
            List<VideoObject> found_selected = new List<VideoObject>();

            foreach (VideoObject video in BatchVideos)
            {
                if (video.Selected == '✅')
                    found_selected.Add(video);
            }

            return found_selected;
        }

        private static bool AddVideoBool = false;
        private async void add_video_btn_Click(object sender, RoutedEventArgs e)
        {
            string video_url = url_box.Text;
            if (Downloader.IsValidURL(video_url) && !AddVideoBool)
            {
                add_video_btn.Content = "Adding video...";
                AddVideoBool = true;
                string video_title = await DlpHandler.GetVideoTitleFromUrl(video_url) ?? "COULDN'T FETCH TITLE";
                VideoObject video = new VideoObject
                {
                    Index = ++batch_index,
                    VideoURL = video_url,
                    VideoName = video_title,
                };
                BatchVideos.Add(video);
                url_box.Text = string.Empty;
                add_video_btn.IsEnabled = false;
                add_video_btn.Content = "Add video";
                AddVideoBool = false;
                remove_vide0_btn.IsEnabled = true;
            }
        }

        private void remove_vide0_btn_Click(object sender, RoutedEventArgs e)
        {
            VideoObject? selected = batch_video_list.SelectedItem as VideoObject;
            if (selected != null)
                BatchVideos.Remove(selected);

            if (BatchVideos.Count == 0)
            {
                remove_vide0_btn.IsEnabled = false;
                download_btn.IsEnabled = false;
                next_btn.IsEnabled = false;
            }
        }

        private void select_all_btn_Click(object sender, RoutedEventArgs e)
        {
            foreach (VideoObject video in BatchVideos)
            {
                video.Selected = '✅';
            }
            batch_video_list.Items.Refresh();
            download_btn.IsEnabled = BatchVideos.Count > 0;
            next_btn.IsEnabled = true;
        }

        private void deselect_all_btn_Click(object sender, RoutedEventArgs e)
        {
            foreach (VideoObject video in BatchVideos)
            {
                video.Selected = 'X';
            }
            batch_video_list.Items.Refresh();
            download_btn.IsEnabled = false;
            next_btn.IsEnabled = false;
        }

        // DOWNLOAD PANEL LOGIC

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
        private void select_selected_btn_Click(object sender, RoutedEventArgs e)
        {
            int selected = batch_video_list.SelectedIndex;
            if (selected >= 0)
                BatchVideos[selected].Selected = '✅';
            batch_video_list.Items.Refresh();
            download_btn.IsEnabled = true;
            next_btn.IsEnabled = true;
        }

        private void deselect_selected_btn_Click(object sender, RoutedEventArgs e)
        {
            int selected = batch_video_list.SelectedIndex;
            if (selected >= 0)
                BatchVideos[selected].Selected = 'X';
            batch_video_list.Items.Refresh();

            download_btn.IsEnabled = GetSelectedVideos().Count > 0;
            next_btn.IsEnabled = download_btn.IsEnabled;
        }

        // BATCH MODE VIEWING SWITCHING LOGIC

        private void previous_btn_Click(object sender, RoutedEventArgs e)
        {
            batch_grid.Visibility = Visibility.Visible;
            options_grid.Visibility = Visibility.Collapsed;
            next_btn.IsEnabled = true;
            previous_btn.IsEnabled = false;
        }

        private void next_btn_Click(object sender, RoutedEventArgs e)
        {
            batch_grid.Visibility = Visibility.Collapsed;
            options_grid.Visibility = Visibility.Visible;
            next_btn.IsEnabled = false;
            previous_btn.IsEnabled = true;

            if (BatchVideos.Count > 0) download_btn.IsEnabled = true;
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

        private void search_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(BatchVideos);
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
    }
}
