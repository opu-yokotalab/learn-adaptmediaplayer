using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace AdaptMediaPlayer
{
    public static class MediaData
    {
        // baseUri : 動画群の実際のファイル（動画ファイル + JSON）の置き場所
        public static Uri baseUri = new Uri(new Uri(System.Windows.Browser.HtmlPage.Document.DocumentUri.AbsoluteUri), "video/");

        // Media : 動画群制御情報
        public class Media
        {
            public string title { get; set; }
            public List<Video> video { get; set; }
            public List<Sync> sync { get; set; }
        }
        // Video : 動画群の中の動画（動画 + 同期ポイント + アニメーション）
        public class Video
        {
            public string url { get; set; }
            public string viewpoint { get; set; }
            public List<MediaData.Rectangle> rectangle { get; set; }
            public List<MediaData.Text> text { get; set; }
        }
        // Rectangle : 四角のアニメーション（変形・移動含む）
        public class Rectangle
        {
            public List<double> height { get; set; }
            public List<double> width { get; set; }
            public string color { get; set; }
            public List<double> left { get; set; }
            public List<double> top { get; set; }
            public List<string> time { get; set; }
        }
        // Text : テキストのアニメーション
        public class Text
        {
            public string text { get; set; }
            public string color { get; set; }
            public List<double> left { get; set; }
            public List<double> top { get; set; }
            public List<string> time { get; set; }
        }
        // Sync : 同期ポイント（時間，タグ（意味））
        public class Sync
        {
            public string tag { get; set; }
            public string time { get; set; }
        }

        // media : 動画群制御情報（JSON形式）の記述を受けるリスト
        public static List<MediaData.Media> media = new List<MediaData.Media>();

        // MovieSet : 動画群をプレイヤー側で管理するクラス
        public class MovieSet
        {
            public Dictionary<string, Controls.MediaPlayer> viewList;
            public Controls.MediaPlayer currentPlayer;
            public int playerPosition;
        }

        // movieList : 今再生（・制御）している動画群のリスト
        public static List<MovieSet> movieList = new List<MovieSet>();
    }
}
