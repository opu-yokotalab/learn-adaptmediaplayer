using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
// 追加分
using System.Runtime.Serialization.Json;
using System.IO;
using System.Json;
using System.Diagnostics;

namespace AdaptMediaPlayer.Views
{
    public class VideoTag
    {
        public TextBlock link { get; set; }
        public List<string> tag { get; set; }
    }

    public partial class SearchView : UserControl
    {
        public static List<VideoTag> videoList = new List<VideoTag>();
        public static Dictionary<string, TextBlock> tagList = new Dictionary<string, TextBlock>();

        public SearchView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var wc = new WebClient();
            wc.OpenReadCompleted += (_s, _e) =>
            {
                if (_e.Error == null)
                {
                    var srJsonList = new StreamReader(_e.Result);
                    var jsonList = new List<string>(srJsonList.ReadToEnd().Split('\n'));

                    foreach (var filename in jsonList)
                    {
                        var videoLink = new TextBlock
                        {
                            Tag = filename + ".json",
                            FontSize = 14
                        };
                        var videotag = new VideoTag
                        {
                            tag = new List<string>(),
                            link = videoLink
                        };
                        videoList.Add(videotag);
                        //Canvas.SetTop(videoLink, 20 * jsonList.IndexOf(filename));

                        var wcJson = new WebClient();
                        wcJson.OpenReadCompleted += (__s, __e) =>
                        {
                            if (__e.Error == null)
                            {
                                JsonValue jsonData;
                                try
                                {
                                    jsonData = JsonValue.Load(__e.Result) as JsonValue;
                                }
                                catch
                                {
                                    (App.Current.RootVisual as MainPage).debugTextBox.Text
                                        = "JSONファイルの記述に誤りがあります．:" + filename;
                                    return;
                                }

                                videoLink.Text = jsonData["title"];
                                foreach (var value in (JsonArray)jsonData["tag"])
                                {
                                    videotag.tag.Add(value);
                                    
                                    // タグリストの作成
                                    if (tagList.Keys.Contains<string>((string)value) == false)
                                    {
                                        var tagLink = new TextBlock
                                        {
                                            Text = value,
                                            FontSize = 14
                                        };
                                        tagLink.MouseLeftButtonDown
                                            += new MouseButtonEventHandler((object ___s, MouseButtonEventArgs ___e) =>
                                        {
                                            foreach (var valueV in videoList)
                                            {
                                                if (valueV.tag.IndexOf(tagLink.Text) != -1)
                                                {
                                                    valueV.link.Visibility = Visibility.Visible;
                                                }
                                                else
                                                {
                                                    valueV.link.Visibility = Visibility.Collapsed;
                                                }
                                            }
                                        });
                                        Canvas.SetLeft(tagLink, 0);
                                        Canvas.SetTop(tagLink, 20 * tagList.Count);
                                        tagLinks.Children.Add(tagLink);
                                        tagList.Add(value, tagLink);
                                    }
                                }
                                
                                videoLink.MouseLeftButtonDown +=
                                    new MouseButtonEventHandler((object ___s, MouseButtonEventArgs ___e) =>
                                {
                                    var mainPage = App.Current.RootVisual as MainPage;
                                    //var tag = (int)(sender as Image).Tag;
                                    var tag = 0;
                                    foreach (var value in MediaData.movieList)
                                    {
                                        if (value.playerPosition == tag)
                                        {
                                            MediaData.media.Remove(value.currentPlayer.media);
                                            mainPage.playerArea.Children.Remove(value.currentPlayer);
                                            MediaData.movieList.Remove(value);
                                            break;
                                        }
                                    }
                                    mainPage.OpenData((string)videoLink.Tag, tag);
                                    this.Visibility = Visibility.Collapsed;
                                });

                            }
                            else
                            {
                                (App.Current.RootVisual as MainPage).debugTextBox.Text += __e.Error.ToString();
                            }
                        };
                        wcJson.OpenReadAsync(new Uri(MediaData.baseUri, filename + ".json"));
                    }
                    foreach (var value in videoList)
                    {
                        Canvas.SetLeft(value.link, 200);
                        Canvas.SetTop(value.link, 20 * videoList.IndexOf(value));
                        result.Children.Add(value.link);
                    }
                }
            };
            wc.OpenReadAsync(new Uri(MediaData.baseUri, "root.txt"));
        }

        public void show()
        {
            this.Visibility = Visibility.Visible;

            /*
            var ser = new DataContractJsonSerializer(typeof(MediaData.Media));
            var ms = new MemoryStream();
            ser.WriteObject(ms, MediaData.media[0]);
            ms.Position = 0;
            var sr = new StreamReader(ms);
            tagList.Text = sr.ReadToEnd();
             */
        }
        public void hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

    }
}
