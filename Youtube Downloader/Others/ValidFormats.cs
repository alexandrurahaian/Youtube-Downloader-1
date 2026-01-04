using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace Youtube_Downloader.Others
{
    public static class ValidFormats
    {
        public static readonly string[] VALID_AUDIO_FORMATS = ["aac", "alac", "flac", "m4a", "mp3", "opus", "vorbis", "wav"];
        public static readonly string[] VALID_VIDEO_FORMATS = ["avi", "flv", "mkv", "mov", "mp4", "webm"];
    }
}
