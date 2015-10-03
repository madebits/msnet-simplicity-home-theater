using System;
using System.Collections.Generic;
using System.Text;

namespace Simplicity
{
    class MovieName
    {
        //http://en.wikipedia.org/wiki/Pirated_movie_release_types
        //http://xick.blogspot.com/2009/04/how-to-interpret-video-file-names.html
        public static string Clean(string fileOrFolder)
        {
            return string.Empty;
            /*
            if (string.IsNullOrEmpty(fileOrFolder)) return string.Empty;
            string s = System.IO.Path.GetFileNameWithoutExtension(fileOrFolder).ToLower().Trim();

            int idx = s.LastIndexOf('-');
            if((idx > 0) && (s.IndexOf('.', idx) < 0))
            {
                s = s.Substring(0, idx);
            }

            int year = DateTime.Now.Year;
            for (int i = 0; i < 5; i++)
            {
                s = s.Replace("." + (year - i) + ".", string.Empty);
            }

            s = s.Replace("dvdscreener", string.Empty);
            s = s.Replace(".wp.", string.Empty);
            s = s.Replace(".ws.", string.Empty);
            s = s.Replace(".fs.", string.Empty);
            s = s.Replace(".ld.", string.Empty);
            s = s.Replace("dvdscr", string.Empty);
            s = s.Replace("svcd", string.Empty);
            s = s.Replace("dvdrip", string.Empty);
            s = s.Replace("dvdr", string.Empty);
            s = s.Replace("pdvd", string.Empty);
            s = s.Replace("dvd.r5", string.Empty);
            s = s.Replace("dvd.r9", string.Empty);
            s = s.Replace("dvd5", string.Empty);
            s = s.Replace("dvd9", string.Empty);
            s = s.Replace("dvd-5", string.Empty);
            s = s.Replace("dvd-9", string.Empty);
            s = s.Replace("dvd", string.Empty);
            s = s.Replace("bdscr", string.Empty);
            s = s.Replace("nfo", string.Empty);
            s = s.Replace("xvid", string.Empty);
            s = s.Replace("divx", string.Empty);
            s = s.Replace("avi", string.Empty);
            s = s.Replace("mpeg-1", string.Empty);
            s = s.Replace("mpeg-2", string.Empty);
            s = s.Replace("mpeg-3", string.Empty);
            s = s.Replace("mpeg-4", string.Empty);
            s = s.Replace("mpeg", string.Empty);
            s = s.Replace("mpg", string.Empty);
            s = s.Replace("blue-ray", string.Empty);
            s = s.Replace("blueray", string.Empty);
            s = s.Replace("bray", string.Empty);
            s = s.Replace("video", string.Empty);
            s = s.Replace("mp3", string.Empty);
            s = s.Replace("ac3", string.Empty);
            s = s.Replace("bdrip", string.Empty);
            s = s.Replace("brrip", string.Empty);
            s = s.Replace("tvrip", string.Empty);
            s = s.Replace("dvbrip", string.Empty);
            s = s.Replace("dthrip", string.Empty);
            s = s.Replace("dsr", string.Empty);
            s = s.Replace("hdtv", string.Empty);
            s = s.Replace("hdtv", string.Empty);
            s = s.Replace("pdtv", string.Empty);
            s = s.Replace("stv", string.Empty);
            s = s.Replace("vhsscr", string.Empty);
            s = s.Replace("vhsrip", string.Empty);
            s = s.Replace("vhs", string.Empty);
            s = s.Replace("bdr", string.Empty);
            s = s.Replace("bd25", string.Empty);
            s = s.Replace("bd50", string.Empty);
            s = s.Replace("bd5", string.Empty);
            s = s.Replace("bd9", string.Empty);
            s = s.Replace("ppvrip", string.Empty);
            s = s.Replace("ppv", string.Empty);
            s = s.Replace("rerip", string.Empty);
            s = s.Replace("rip", string.Empty);
            s = s.Replace("p2p", string.Empty);
            s = s.Replace("r5", string.Empty);
            s = s.Replace("x264", string.Empty);
            s = s.Replace("pal", string.Empty);
            s = s.Replace("ntsc", string.Empty);
            s = s.Replace("720p", string.Empty);
            s = s.Replace("1080p", string.Empty);
            s = s.Replace("1080i", string.Empty);
            s = s.Replace("mp4", string.Empty);
            s = s.Replace("ogg", string.Empty);
            s = s.Replace("mkv", string.Empty);
            s = s.Replace("unsubbed", string.Empty);
            s = s.Replace("subbed", string.Empty);
            s = s.Replace("..", string.Empty);
            s = s.Replace("-", " ");
            s = s.Replace(".", " ");
            s = s.Replace("!", " ");
            s = s.Replace("+", " ");
            s = s.Replace("#", " ");
            s = s.Replace("~", " ");
            s = s.Replace("_", " ");
            s = s.Replace("=", " ");
            s = s.Replace("&", " ");
            s = s.Replace("$", " ");
            s = s.Replace(",", " ");
            s = s.Replace("'", " ");
            s = s.Replace("|", " ");
            s = s.Replace("(", " ");
            s = s.Replace(")", " ");
            s = s.Replace("[", " ");
            s = s.Replace("]", " ");
            s = s.Replace("{", " ");
            s = s.Replace("}", " ");
            return s.Trim();
            */
        }

    }
}
