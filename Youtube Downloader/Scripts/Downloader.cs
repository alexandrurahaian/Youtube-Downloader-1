using System;
using System.Collections.Generic;
using System.Text;

namespace Youtube_Downloader.Scripts
{
    public enum DownloadType
    {
        SingleVideo,
        BatchVideo,
        Playlist
    }

    public enum DownloadOptions
    {
        VideoOnly,
        AudioOnly,
        AudioVideo
    }

    public class Downloader
    {
        DownloadType downloadType;
        DownloadOptions downloadOptions;
        string fileFormat, quality, path;
        string[] downloadUrls;

        public Downloader(DownloadType d_type, string file_format, string videoQuality, string d_path, DownloadOptions d_option, string[] urls)
        {
            downloadType = d_type;
            downloadOptions = d_option;
            fileFormat = file_format;
            quality = videoQuality;
            downloadUrls = urls;
            path = d_path;
        }

        public (bool, string?) Download(bool is_playlist = false)
        {
            try
            {
                string[] arguments_list = ParseArgumentsList(fileFormat, quality, path, downloadUrls, downloadType, downloadOptions);

                DlpHandler.StartDlpProcess(arguments_list, is_playlist);
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, $"{e.Message}\n{e.StackTrace}\n{e.InnerException}");
            }
        }

        private static string[] ParseArgumentsList(string file_format, string videoQuality, string outputPath, string[] urls, DownloadType d_type, DownloadOptions d_options)
        {
            List<string> parsed_arguments = new List<string>();

            bool forceAudioOnly = file_format is "mp3" or "wav" or "flac" or "aac" or "3ga";
            bool forceReencodeAudio = file_format is "mp3" or "wav" or "flac" or "3ga";

            if (file_format == "3ga" && d_options != DownloadOptions.AudioOnly)
                throw new InvalidOperationException("3GA is audio-only and requires re-encoding.");
            parsed_arguments.Add("-f");
            switch (d_options)
            {
                case DownloadOptions.AudioOnly:
                    parsed_arguments.Add(videoQuality == "best" ? "ba" : "wa");
                    parsed_arguments.Add("-x");
                    parsed_arguments.Add($"--audio-format {file_format}");
                    break;

                case DownloadOptions.VideoOnly:
                    parsed_arguments.Add(videoQuality == "best" ? "bv" : "wv");
                    if (file_format is "mp4" or "mkv" or "webm")
                        parsed_arguments.Add($"--remux-video {file_format}");
                    else
                        parsed_arguments.Add($"--recode-video {file_format}");
                    break;
                case DownloadOptions.AudioVideo:
                default:
                    parsed_arguments.Add(videoQuality == "best" ? "b" : "w");
                    parsed_arguments.Add($"--merge-output-format {file_format}");
                    break;
            }

            parsed_arguments.Add($"--no-warnings");
            parsed_arguments.Add("-P");
            parsed_arguments.Add($"\"{outputPath}\"");
            parsed_arguments.AddRange(urls);
            return parsed_arguments.ToArray();
        }

        public static bool IsValidURL(string url)
        {
            if (url.Contains("youtube.com") || url.Contains("youtu.be")) return true;
            return false;
        }
    }
}
