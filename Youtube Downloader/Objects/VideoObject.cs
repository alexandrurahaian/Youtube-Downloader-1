using System;
using System.Collections.Generic;
using System.Text;

namespace Youtube_Downloader.Objects
{
    public class VideoObject
    {
        public int Index { get; set; }
        public string VideoName { get; set; }
        public string VideoURL { get; set; }
        public char Selected { get; set; } = 'X'; // ✅
    }
}
