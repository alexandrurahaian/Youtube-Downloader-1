using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Youtube_Downloader.Objects;
using Youtube_Downloader.Pages;

namespace Youtube_Downloader.Scripts
{
    public static class DlpHandler
    {
        public static Process? currentDLP_Process = null;
        private static readonly string FFmpegPath = Path.Combine(AppContext.BaseDirectory, "FFmpeg");
        public static Video video_page;
        public static Playlist playlist_page;
        private static bool IsPlaylist = false;
        public static (bool, string?, StreamReader?) StartDlpProcess(string[] arguments, bool is_playlist = false)
        {
            if (currentDLP_Process != null) return (false, "A yt-dlp process is already running!", null);
            string dlp_path = Path.Combine(AppContext.BaseDirectory, "yt-dlp.exe");
            
            try
            {
                string arg_str = (is_playlist ? $"--ffmpeg-location \"{FFmpegPath}\" -o %(title)s.%(ext)s --newline {string.Join(" ", arguments)} --yes-playlist" : $"--ffmpeg-location \"{FFmpegPath}\" -o %(title)s.%(ext)s --newline {string.Join(" ", arguments)}");
                Process dlp_proc = new Process();
                dlp_proc.StartInfo.FileName = dlp_path;
                dlp_proc.StartInfo.Arguments = arg_str;
                dlp_proc.Exited += Dlp_proc_Exited;
                Debug.WriteLine(dlp_proc.StartInfo.Arguments);
                dlp_proc.StartInfo.RedirectStandardOutput = true;
                dlp_proc.StartInfo.RedirectStandardError = true;
                dlp_proc.StartInfo.UseShellExecute = false;
                dlp_proc.StartInfo.CreateNoWindow = true;
                IsPlaylist = is_playlist;

                dlp_proc.OutputDataReceived += (sender, e) =>
                {
                    Debug.WriteLine($"Data: {e.Data}");
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        string data = e.Data;
                        
                        if (data.StartsWith("[download]") && data.Contains("of"))
                        {
                            if (!is_playlist)
                                video_page.UpdateProgress = true;
                            else playlist_page.UpdateProgress = true;

                            var match = Regex.Match(data,
                                @"\[download\]\s+([\d.]+)%.*?of\s+([\d.]+\w+).*?at\s+([\w./]+).*?ETA\s+([\w:]+)");

                            if (match.Success)
                            {
                                double progress = double.Parse(match.Groups[1].Value);
                                string size = match.Groups[2].Value;
                                string speed = match.Groups[3].Value;
                                string eta = match.Groups[4].Value;
                                //int currentFragment = int.Parse(match.Groups[5].Value);
                                //int totalFragments = int.Parse(match.Groups[6].Value);
                                Debug.WriteLine("Sucess");
                                if (!is_playlist)
                                {
                                    video_page.Dispatcher.Invoke(() =>
                                    {
                                        video_page.UpdateProgress = true;
                                        video_page.DisplayProgress(progress, size, speed, eta, 1, 1);
                                    });
                                    Debug.WriteLine("Fired display progress on video");
                                }
                                else
                                {
                                    playlist_page.Dispatcher.Invoke(() =>
                                    {
                                        playlist_page.UpdateProgress = true;
                                        playlist_page.DisplayProgress(progress, size, speed, eta, 1, 1);
                                    });
                                    Debug.WriteLine("Fired display progress on playlist");
                                }
                            }
                        }
                        else if (data.Contains("has already been downloaded"))
                        {
                            if (!is_playlist)
                            {
                                video_page.Dispatcher.Invoke(() =>
                                {
                                    video_page.download_btn.IsEnabled = true;
                                    video_page.download_error_display.Visibility = Visibility.Visible;
                                    video_page.download_error_display.Text = $"Download failed: file already exists.";
                                    video_page.progress_panel.Visibility = Visibility.Collapsed;
                                    video_page.progress_details_display.Text = "Starting yt-dlp.exe...";
                                });
                            }
                            else
                            {
                                playlist_page.Dispatcher.Invoke(() =>
                                {
                                    playlist_page.download_btn.IsEnabled = true;
                                    playlist_page.download_error_display.Visibility = Visibility.Visible;
                                    playlist_page.download_error_display.Text = $"Download failed: file already exists.";
                                    playlist_page.progress_panel.Visibility = Visibility.Collapsed;
                                    playlist_page.progress_details_display.Text = "Starting yt-dlp.exe...";
                                });
                            }
                        }
                        else if (data.StartsWith("[download]"))
                        {
                            if (!is_playlist)
                                video_page.UpdateProgress = true;
                            else
                                playlist_page.UpdateProgress = true;
                        }
                    }
                };

                dlp_proc.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        if (e.Data.Contains("data blocks"))
                        {
                            if (is_playlist) playlist_page.DisplayProgress(100, "", "", "", 1, 1, true);
                            else video_page.DisplayProgress(100, "", "", "", 1, 1, true);
                        }
                        else
                            MessageBox.Show($"yt-dlp.exe has enountered an error:\n{e.Data}");
                    }
                };

                dlp_proc.EnableRaisingEvents = true;
                dlp_proc.Start();
                dlp_proc.BeginOutputReadLine();
                dlp_proc.BeginErrorReadLine();

                currentDLP_Process = dlp_proc;
                return (true, null, dlp_proc.StandardOutput);
            }
            catch (Exception ex)
            {
                return (false, $"{ex.Message}\n{ex.StackTrace}\n{ex.InnerException}", null);
            }
        }

        private static void Dlp_proc_Exited(object? sender, EventArgs e)
        {
            currentDLP_Process = null;
            if (IsPlaylist)
            {
                playlist_page.Dispatcher.Invoke(() =>
                {
                    playlist_page.UpdateProgress = false;
                    playlist_page.progress_panel.Visibility = Visibility.Collapsed;
                    playlist_page.download_finish_panel.Visibility = Visibility.Visible;
                    playlist_page.download_btn.IsEnabled = true;
                });
            }
            else
            {
                video_page.Dispatcher.Invoke(() =>
                {
                    video_page.UpdateProgress = false;
                    video_page.progress_panel.Visibility = Visibility.Collapsed;
                    video_page.download_finish_panel.Visibility = Visibility.Visible;
                    video_page.download_btn.IsEnabled = true;
                });
            }
        }

        public static (bool, string?) KillDlpProcess()
        {
            if (currentDLP_Process == null) return (true, null);
            try
            {
                currentDLP_Process.Kill(true);
                currentDLP_Process = null;
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"{ex.Message}\n{ex.StackTrace}\n{ex.InnerException}");
            }
        }

        public static async Task<string?> GetVideoTitleFromUrl(string url)
        {
            if (currentDLP_Process != null) KillDlpProcess();

            string dlp_path = Path.Combine(AppContext.BaseDirectory, "yt-dlp.exe");
            Process dlp_proc = new Process();
            dlp_proc.StartInfo.FileName = dlp_path;
            dlp_proc.StartInfo.Arguments = $"--simulate --print \"%(title)s\" --no-warnings {url}";
            dlp_proc.StartInfo.RedirectStandardOutput = true;
            dlp_proc.StartInfo.RedirectStandardError = true;
            dlp_proc.StartInfo.UseShellExecute = false;
            dlp_proc.StartInfo.CreateNoWindow = true;

            dlp_proc.Start();

            string? title = await dlp_proc.StandardOutput.ReadLineAsync();

            dlp_proc.WaitForExit();
            return title;
        }

        public static async Task<List<VideoObject>> GetPlaylistVideos(string playlist_url)
        {
            if (currentDLP_Process != null) KillDlpProcess();

            List<VideoObject> fetched_video_objects = new List<VideoObject>();

            string dlp_path = Path.Combine(AppContext.BaseDirectory, "yt-dlp.exe");
            Process dlp_proc = new Process();
            dlp_proc.StartInfo.FileName = dlp_path;
            dlp_proc.StartInfo.Arguments = $"{playlist_url} --print \"%(title)s|||&*|%(url)s\" --flat-playlist --match-title \"^(?!\\\\[Deleted video\\\\]|\\\\[Private video\\\\]).*$\"";
            dlp_proc.StartInfo.RedirectStandardOutput = true;
            dlp_proc.StartInfo.RedirectStandardError = true;
            dlp_proc.StartInfo.UseShellExecute = false;
            dlp_proc.StartInfo.CreateNoWindow = true;
            dlp_proc.EnableRaisingEvents = true;
            dlp_proc.Start();

            int count = 0;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            dlp_proc.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    string data = e.Data;
                    string[] split_dat = data.Split("|||&*|", 2);
                    Debug.WriteLine($"Checking: {split_dat[0]} - {split_dat[1]}");
                    if (split_dat.Length >= 2 && (split_dat[0] is "[Deleted video]" or "[Private video]" == false))
                    {
                        fetched_video_objects.Add(new VideoObject()
                        {
                            VideoName = split_dat[0],
                            VideoURL = split_dat[1],
                            Selected = '✅',
                            Index = ++count
                        });
                        Debug.WriteLine($"Added: {split_dat[0]} - {split_dat[1]}");
                    }
                    
                }
            };

            dlp_proc.BeginOutputReadLine();
            dlp_proc.BeginErrorReadLine();

            dlp_proc.Exited += (sender, e) =>
            {
                tcs.TrySetResult(true);
                currentDLP_Process = null;
            };

            await tcs.Task;
            Debug.WriteLine($"Sent");
            Debug.WriteLine($"Count: {fetched_video_objects.Count}");
            return fetched_video_objects;
        }
    }
}
