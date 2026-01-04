# Youtube Downloader

Easily download playlists and videos from Youtube.<br>
[Install latest](https://github.com/vortex3225/Youtube-Downloader/releases/latest)

# Usage
Select the download mode (playlist or video)
<img  align="left" width="293" height="57" alt="image" src="https://github.com/user-attachments/assets/c766d81d-9218-464e-8e8b-f4f661ef0c80" />
<br>
<br>

## Playlist downloading
<img align="left" width="574" height="397" alt="image" src="https://github.com/user-attachments/assets/4f681d60-657f-4df3-9b6e-d55854c735c6" />

Input a valid Youtube playlist URL which is **public or unlisted**<br>
After that, press the Load playlist button which will load all the videos in the playlist.<br>
>**Loading large playlists might take longer**<br>

By default, all the videos loaded are automatically selected. You can choose which videos to download by deselecting all the videos and manually reselecting the ones you want to download.<br>
Once you are done, press the *Next* button to open the [download panel](#downloading).
<br>
<br>
<br>
<br>
<br>
## Video downloading
There 2 ways to download videos, single vide0 mode which only downloads a [single video](#single-video-download) and batch video mode for downloading multiple videos.
### Single video download
<img align="left" width="578" height="211" alt="image" src="https://github.com/user-attachments/assets/b8284157-99f2-4c3b-a038-c94b7c9fe7b9" />
<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>

In single video mode, you can only download 1 Youtube video which is **public or unlisted**.

### Batch video download
<img align="left" width="568" height="325" alt="image" src="https://github.com/user-attachments/assets/6449f4f9-8ea6-4ac3-9669-862ae5929348" />
<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>
<br>
In batch video mode, you can input multiple Youtube video URLs as long as the videos are public or unlisted.<br>
<br>
<br>
<img align="left" width="557" height="70" alt="image" src="https://github.com/user-attachments/assets/f7e6d448-d847-4700-bfe6-cfed5833de33" />
<br>
<br>
<br>
<br>
Press the Add video button to add a video to the list.<br>

> By default, videos added to the list are not selected automatically.

Added videos are displayed in the list by their Index (starting from 1), video title, video URL and selection (X means the video is not selected while a checkbox means the video is selected). <br>
Only one video can be selected at a time.

To remove a video from the list, select a video and press the Remove video button.
<br>
<img align="left" width="317" height="74" alt="image" src="https://github.com/user-attachments/assets/ef989721-fb74-4bb9-8674-b8e839d889cd" />
<br>
<br>
<br>
<br>
<br>
<br>

Once you select atleast 1 video, you can progress to the [download page](#downloading)

# Downloading
<img align="left" width="574" height="180" alt="image" src="https://github.com/user-attachments/assets/4e690614-d478-4951-bf38-9085839b00d8" />

> Download page for playlist and batch/single video downloads.

<br>
<br>
<br>
<br>
<br>
<br>
<br>

## File Format selection
The available file formats depend on the download mode you selected. (audio only, video only or audio-video)<br>
The valid file formats are the ones [yt-dlp](https://github.com/yt-dlp/yt-dlp) supports currently.
The downloader will attempt to prevent loss of quality by remuxing instead of re-encoding if the format allows this.<br>
### Supported file formats
```
AUDIO: aac, alac, flac, m4a, mp3, opus, vorbis, wav
VIDEO (video only and audio-video): avi, flv, mkv, mov, mp4, webm
```

<br>

### Quality
The 2 available quality modes are best and worst.<br>

> *best* will attempt to download the highest quality possible for the selected video.<br>
> *worst* will attempt to download the lowest quality possible for the selected video.

For more information view the [yt-dlp format selection documentation](https://github.com/yt-dlp/yt-dlp?tab=readme-ov-file#format-selection)<br>

Once a download is started, yt-dlp.exe will be started and the video/videos will be downloaded in the specified download location.<br>
A progress bar will appear which will indicate the download progress of the current fragment, along with the estimated time until completion and download speed.
