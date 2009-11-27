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
using System.Json;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using AdaptMediaPlayer.Views;

namespace AdaptMediaPlayer
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 変数，クラスの初期化
            //MediaData.sb.Duration = TimeSpan.FromMinutes(30);  // 時間は適当

            // タイマー
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(200);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            #region スロー再生用タイマー
            /*
            var timerSlow = new System.Windows.Threading.DispatcherTimer();
            timerSlow.Interval = TimeSpan.FromMilliseconds(100);
            timerSlow.Tick += new EventHandler(timerSlow_Tick);
            timerSlow.Start();
             */
            #endregion

            // 適当にデフォルトの状態を指定
            //var jsonList = new List<string>{
            //    "otehon.json",
            //    "student.json"
            //};

            var jsonList = new List<string>();
            var wc = new WebClient();
            wc.DownloadStringCompleted += (_s, _e) =>
            {
                jsonList = _e.Result.Trim().Split('\n').ToList<string>();
                
                // 適当に指定したファイル読み込み
                foreach (var filename in jsonList)
                {
                    OpenData(filename, jsonList.IndexOf(filename));
                }
            };
            wc.DownloadStringAsync(new Uri(MediaData.baseUri, "input.txt"));

        }

        public void OpenData(string filename, int number)
        {
            // JSON読み込み
            var wc = new WebClient();
            wc.OpenReadCompleted += (_s, _e) =>
            {
                if (_e.Error == null)
                {
                    // JSONファイルをMediaクラスのインスタンスに逆シリアル化
                    var ser = new DataContractJsonSerializer(typeof(MediaData.Media));
                    try
                    {
                        MediaData.media.Add((MediaData.Media)ser.ReadObject(_e.Result));
                        debugTextBox.Text = string.Empty;
                    }
                    catch
                    {
                        debugTextBox.Text = "JSONファイルの記述に誤りがあります．:" + filename;
                        return;
                    }

                    // 動画ソースへ
                    var movieSet = new MediaData.MovieSet();
                    movieSet.viewList = new Dictionary<string, AdaptMediaPlayer.Controls.MediaPlayer>();
                    movieSet.playerPosition = number;

                    for (var urlPos = 0; urlPos < MediaData.media[MediaData.media.Count - 1].video.Count; urlPos++)
                    {
                        var player = new Controls.MediaPlayer();

                        player.sb = new Storyboard
                        {
                            Duration = TimeSpan.FromMinutes(30) // 時間は適当
                        };

                        // Mediaとひも付け
                        player.media = MediaData.media[MediaData.media.Count - 1];

                        player.movie.Source = new Uri(MediaData.baseUri, player.media.video[urlPos].url);
                        player.title.Text = player.media.title;
                        player.movie_index = MediaData.media.Count - 1;
                        player.Margin = new Thickness(8, 8, 0, 0);
                        Canvas.SetLeft(player, 340 * number);
                        Canvas.SetTop(player, 0);

                        // JSONからアニメーションを生成
                        // rectangle編
                        foreach (var value in player.media.video[0].rectangle) // 決め打ち
                        {
                            // 四角を作る
                            var rect = new Rectangle
                            {
                                Height = value.height[0],
                                Width = value.width[0],
                                Stroke = new SolidColorBrush(TransColor(value.color)),
                                StrokeThickness = 2,
                                Visibility = Visibility.Collapsed
                            };
                            Canvas.SetLeft(rect, value.left[0]);
                            Canvas.SetTop(rect, value.top[0]);

                            player.anime.Children.Add(rect);

                            // アニメの定義
                            var anim = new ObjectAnimationUsingKeyFrames();

                            // 四角を出す
                            var frame1 = new DiscreteObjectKeyFrame();
                            frame1.KeyTime = TimeSpan.Zero;
                            frame1.Value = Visibility.Visible;

                            // 四角を消す
                            var frame2 = new DiscreteObjectKeyFrame();
                            frame2.KeyTime = TimeSpan.Parse(value.time[value.time.Count - 1]) - TimeSpan.Parse(value.time[0]);
                            frame2.Value = Visibility.Collapsed;

                            anim.KeyFrames.Add(frame1);
                            anim.KeyFrames.Add(frame2);

                            anim.BeginTime = TimeSpan.Parse(value.time[0]);

                            // Storyboardの定義
                            player.sb.Children.Add(anim);
                            Storyboard.SetTarget(anim, rect);
                            Storyboard.SetTargetProperty(anim, new PropertyPath(VisibilityProperty));

                            // 長方形の変形・移動を設定する
                            if (value.height.Count >= 2)
                            {
                                var animeHeight = new DoubleAnimationUsingKeyFrames();
                                var animeWidth = new DoubleAnimationUsingKeyFrames();
                                var animeLeft = new DoubleAnimationUsingKeyFrames();
                                var animeTop = new DoubleAnimationUsingKeyFrames();

                                for (var i = 1; i < value.time.Count; i++)
                                {
                                    // 四角の変形 : Height
                                    var animeHeightKeyFrame = new LinearDoubleKeyFrame
                                    {
                                        KeyTime = TimeSpan.Parse(value.time[i]),
                                        Value = value.height[i]
                                    };
                                    animeHeight.KeyFrames.Add(animeHeightKeyFrame);

                                    // 四角の変形 : Width
                                    var animeWidthKeyFrame = new LinearDoubleKeyFrame
                                    {
                                        KeyTime = TimeSpan.Parse(value.time[i]),
                                        Value = value.width[i]
                                    };
                                    animeWidth.KeyFrames.Add(animeWidthKeyFrame);

                                    // 四角の移動 : Left
                                    var animeLeftKeyFrame = new LinearDoubleKeyFrame
                                    {
                                        KeyTime = TimeSpan.Parse(value.time[i]),
                                        Value = value.left[i]
                                    };
                                    animeLeft.KeyFrames.Add(animeLeftKeyFrame);

                                    // 四角の移動 : Top
                                    var animeTopKeyFrame = new LinearDoubleKeyFrame
                                    {
                                        KeyTime = TimeSpan.Parse(value.time[i]),
                                        Value = value.top[i]
                                    };
                                    animeTop.KeyFrames.Add(animeTopKeyFrame);
                                }
                                player.sb.Children.Add(animeHeight);
                                Storyboard.SetTarget(animeHeight, rect);
                                Storyboard.SetTargetProperty(animeHeight, new PropertyPath(Rectangle.HeightProperty));

                                player.sb.Children.Add(animeWidth);
                                Storyboard.SetTarget(animeWidth, rect);
                                Storyboard.SetTargetProperty(animeWidth, new PropertyPath(Rectangle.WidthProperty));

                                player.sb.Children.Add(animeLeft);
                                Storyboard.SetTarget(animeLeft, rect);
                                Storyboard.SetTargetProperty(animeLeft, new PropertyPath(Canvas.LeftProperty));

                                player.sb.Children.Add(animeTop);
                                Storyboard.SetTarget(animeTop, rect);
                                Storyboard.SetTargetProperty(animeTop, new PropertyPath(Canvas.TopProperty));
                            }
                        }

                        // text編
                        foreach (var value in player.media.video[0].text) // 決め打ち
                        {
                            // テキストを作る
                            var str = new TextBlock
                            {
                                Text = value.text,
                                FontSize = 18,
                                Foreground = new SolidColorBrush(TransColor(value.color)),
                                Visibility = Visibility.Collapsed
                            };
                            Canvas.SetLeft(str, value.left[0]);
                            Canvas.SetTop(str, value.top[0]);

                            player.anime.Children.Add(str);

                            // アニメの定義
                            var anim = new ObjectAnimationUsingKeyFrames();

                            // テキストを出す
                            var frame1 = new DiscreteObjectKeyFrame();
                            frame1.KeyTime = TimeSpan.Zero;
                            frame1.Value = Visibility.Visible;

                            // テキストを消す
                            var frame2 = new DiscreteObjectKeyFrame();
                            frame2.KeyTime = TimeSpan.Parse(value.time[1]) - TimeSpan.Parse(value.time[0]);
                            frame2.Value = Visibility.Collapsed;

                            anim.KeyFrames.Add(frame1);
                            anim.KeyFrames.Add(frame2);

                            anim.BeginTime = TimeSpan.Parse(value.time[0]);

                            // Storyboardの定義
                            player.sb.Children.Add(anim);
                            Storyboard.SetTarget(anim, str);
                            Storyboard.SetTargetProperty(anim, new PropertyPath(VisibilityProperty));
                        }

                        // プレイヤーを追加
                        playerArea.Children.Add(player);

                        // 最初に表示するプレイヤーを決定
                        movieSet.viewList.Add(player.media.video[urlPos].viewpoint, player);
                        if (urlPos == 0)
                        {
                            movieSet.currentPlayer = player;
                        }
                        else
                        {
                            player.Visibility = Visibility.Collapsed;
                        }
                    }
                    MediaData.movieList.Add(movieSet);

                    // カメラ切替ボタン生成
                    if (MediaData.media.Count == 1)
                    {
                        foreach (var value in MediaData.media[0].video) // 決め打ち
                        {
                            var buttonChange = new Button
                            {
                                Content = value.viewpoint,
                                FontSize = 18,
                                Width = 80
                            };
                            Canvas.SetTop(buttonChange, 35 * MediaData.media[0].video.IndexOf(value)); // 決め打ち
                            buttonChange.Click += new RoutedEventHandler(ChangeCamera);
                            ChangeCameraArea.Children.Add(buttonChange);

                            var buttonEx = new Button
                            {
                                Content = value.viewpoint,
                                FontSize = 18,
                                Width = 80
                            };
                            Canvas.SetTop(buttonEx, 35 * (MediaData.media[0].video.IndexOf(value) + 1)); // 決め打ち
                            buttonEx.Click += new RoutedEventHandler(ExCamera);
                            ExCamaraArea.Children.Add(buttonEx);
                        }
                    }

                    // JSON変更ボタン
                    var openJsonButton = new Image
                    {
                        Source = new BitmapImage(new Uri("../image/Open.png", UriKind.Relative)),
                        Height = 24,
                        Width = 24,
                        Margin = new Thickness(8, 8, 0, 0),
                        Tag = movieSet.playerPosition
                    };
                    Canvas.SetLeft(openJsonButton, 288 + 340 * movieSet.playerPosition);
                    openJsonButton.MouseLeftButtonDown += new MouseButtonEventHandler(OpenJsonFile);
                    openFileButtonArea.Children.Add(openJsonButton);
                }
                else
                {
                    // エラー処理
                    debugTextBox.Text = _e.Error.ToString();
                }
            };
            wc.OpenReadAsync(new Uri(MediaData.baseUri, filename));

            #region スロー再生（現在不使用）
            //for (var j = 0; j < MediaData.media[0].sync.Count; j++)
            //{
            //    double ratio;

            //    if (j == 0)
            //    {
            //        ratio = MediaData.media[0].sync[j].TotalMilliseconds
            //                    / MediaData.media[1].sync[j].TotalMilliseconds;
            //    }
            //    else
            //    {
            //      ratio = (MediaData.media[0].sync[j].TotalMilliseconds - MediaData.media[0].sync[j - 1].TotalMilliseconds)
            //                    / (MediaData.media[1].sync[j].TotalMilliseconds - MediaData.media[1].sync[j - 1].TotalMilliseconds);
            //    }

            //    MediaData.movieList[0].currentPlayer.slowPlayRatio.Add(ratio);
            //    //Debug.WriteLine(ratio);
            //    /*
            //    double ratio;
            //    int big, small;

            //    if (j == 0)
            //    {
            //        if (MediaData.media[0].sync[j] < MediaData.media[1].sync[j])
            //        {
            //            big = 1;
            //            small = 0;
            //        }
            //        else
            //        {
            //            big = 0;
            //            small = 1;
            //        }

            //        ratio = MediaData.media[small].sync[j].TotalMilliseconds
            //                    / MediaData.media[big].sync[j].TotalMilliseconds;
            //    }
            //    else
            //    {
            //        if (MediaData.media[0].sync[j] - MediaData.media[0].sync[j - 1]
            //            < (MediaData.media[1].sync[j] - MediaData.media[1].sync[j - 1]))
            //        {
            //            big = 1;
            //            small = 0;
            //        }
            //        else
            //        {
            //            big = 0;
            //            small = 1;
            //        }
            //        ratio = (MediaData.media[small].sync[j].TotalMilliseconds - MediaData.media[small].sync[j - 1].TotalMilliseconds)
            //                    / (MediaData.media[big].sync[j].TotalMilliseconds - MediaData.media[big].sync[j - 1].TotalMilliseconds);
            //    }

            //    MediaData.movieList[small].currentPlayer.slowPlayRatio.Add(ratio);
            //    MediaData.movieList[big].currentPlayer.slowPlayRatio.Add(1.0);
            //     */
            //}
            /*
            for (var j = 0; j < MediaData.movieList[0].currentPlayer.slowPlayRatio.Count; j++)
            {
                Debug.WriteLine(MediaData.movieList[0].currentPlayer.slowPlayRatio[j]);
                Debug.WriteLine(MediaData.movieList[1].currentPlayer.slowPlayRatio[j]);
            }
             */
            #endregion
        }

        void timer_Tick(object sender, EventArgs e)
        {
            foreach (var value in MediaData.movieList)
            {
                // バッファリングの進捗（IEでしかまともに分からない？）
                if (value.currentPlayer.movie.CurrentState == MediaElementState.Buffering)
                {
                    value.currentPlayer.bufferProgress.Text = ((int)(value.currentPlayer.movie.BufferingProgress * 100)).ToString() + "%";
                }

                // 動画のカウンタを更新
                value.currentPlayer.position.Text = value.currentPlayer.movie.Position.ToString();
                // シークバーを更新
                value.currentPlayer.progressBar.Width = value.currentPlayer.movie.Position.TotalSeconds
                                            / value.currentPlayer.movie.NaturalDuration.TimeSpan.TotalSeconds * 320;
            }
        }

        #region スロー再生用
        //void timerSlow_Tick(object sender, EventArgs e)
        //{
        //        if (MediaData.IsSlowPlay)
        //        {
        //            MediaData.movieList[0].currentPlayer.movie.Position
        //                += TimeSpan.FromMilliseconds(100 * MediaData.movieList[0].currentPlayer.currentPlayRatio);
        //            /*
        //            foreach (var value in MediaData.movieList)
        //            {
        //                if (value.currentPlayer.currentPlayRatio != 1.0)
        //                {
        //                    value.currentPlayer.movie.Position
        //                        += TimeSpan.FromMilliseconds(100 * value.currentPlayer.currentPlayRatio);
        //                }
        //            }
        //             */
        //        }
        //}
        #endregion

        // 停止
        private void StopMediaAll(object sender, MouseButtonEventArgs e)
        {
            //MediaData.IsSlowPlay = false;

            foreach (var value in MediaData.movieList)
            {
                value.currentPlayer.StopMedia(sender, e);
            }
        }
        // 一時停止
        private void PauseMediaAll(object sender, MouseButtonEventArgs e)
        {
            foreach (var value in MediaData.movieList)
            {
                value.currentPlayer.PauseMedia(sender, e);
            }
        }
        // 再生
        private void PlayMediaAll(object sender, MouseButtonEventArgs e)
        {
            //MediaData.IsSlowPlay = false;

            foreach (var value in MediaData.movieList)
            {
                value.currentPlayer.PlayMedia(sender, e);
            }
        }

        // コマ送り
        private void FaMediaAll(object sender, MouseButtonEventArgs e)
        {
            foreach (var value in MediaData.movieList)
            {
                value.currentPlayer.movie.Position += TimeSpan.FromMilliseconds(200);
                value.currentPlayer.movieEx.Position += TimeSpan.FromMilliseconds(200);

                if (value.currentPlayer.sb.GetCurrentState() == ClockState.Stopped)
                {
                    value.currentPlayer.sb.Begin();
                    value.currentPlayer.sb.Pause();
                }
                value.currentPlayer.sb.SeekAlignedToLastTick(MediaData.movieList[0].currentPlayer.movie.Position);
            }
        }

        #region スロー再生（現在不使用）
        // スロー再生　→　ロードが追いつかない，負荷が高すぎる　→　使い物にならない
        // WPFの再生速度変更プロパティがSilverlightにもあれば…
        //private void SlowMediaAll(object sender, MouseButtonEventArgs e)
        //{
        //    //foreach (var value in MediaData.movieList)
        //    //{
        //    //    value.currentPlayer.movie.Pause();
        //    //    value.currentPlayer.currentPlayRatio = value.currentPlayer.slowPlayRatio[0];
        //    //    if (value.currentPlayer.currentPlayRatio == 1.0)
        //    //    {
        //    //        value.currentPlayer.movie.Play();
        //    //    }
        //    //    //Debug.WriteLine(value.currentPlayer.currentPlayRatio);
        //    //}
        //    MediaData.movieList[0].currentPlayer.movie.Pause();
        //    MediaData.movieList[0].currentPlayer.currentPlayRatio = MediaData.movieList[0].currentPlayer.slowPlayRatio[0];
        //    MediaData.movieList[1].currentPlayer.movie.Play();
        //    MediaData.IsSlowPlay = true;
        //}
        #endregion

        // 長方形のプロパティを表示（デバッグ用）
        private void OutputDebugRect(object sender, MouseButtonEventArgs e)
        {
            if (MediaData.movieList[0].currentPlayer.drawArea.FindName("hoge") != null)
            {
                Rectangle ele = (Rectangle)MediaData.movieList[0].currentPlayer.drawArea.FindName("hoge");

                var json_obj = new JsonObject();
                json_obj.Add("height", ele.Height.ToString());
                json_obj.Add("width", ele.Width.ToString());
                //json_obj.Add("color", "red");
                json_obj.Add("left", Canvas.GetLeft(ele).ToString());
                json_obj.Add("top", Canvas.GetTop(ele).ToString());
                json_obj.Add("start", MediaData.movieList[0].currentPlayer.movie.Position.ToString());
                //json_obj.Add("end", (movie1.Position + TimeSpan.FromSeconds(2)).ToString());
                debugTextBox.Text = json_obj.ToString();
                //debugTextBox.Text += json_obj.ToString() + "\n";
            }
        }

        // JSONファイルをダイアログから指定して読み込み
        // 今はファイル名しか見てません（中身を考慮してません）
        private void OpenJsonFile(object sender, MouseButtonEventArgs e)
        {
            var opener = new OpenFileDialog();
            opener.Filter = "動画情報ファイル(*.json)|*.json";

            if (opener.ShowDialog() != false)
            {
                var tag = (int)(sender as Image).Tag;
                foreach (var value in MediaData.movieList)
                {
                    if (value.playerPosition == tag)
                    {
                        //MediaData.media.Remove(value.currentPlayer.media);
                        playerArea.Children.Remove(value.currentPlayer);
                        MediaData.movieList.Remove(value);
                        break;
                    }
                }
                OpenData(opener.File.Name, tag);
            }
        }

        /*

        private void ClearDebugRect(object sender, RoutedEventArgs e)
        {
            drawArea.canvas.Children.Remove((UIElement)drawArea.FindName("hoge"));
            drawArea.drawingRectangle = null;
        }

        private void ClearDebugText(object sender, RoutedEventArgs e)
        {
            debugTextBox.Text = "";
        }

        // 変更時のタイトル変更がまだ　→　できるようになった
        // 変更時にmovieのindexも合わせて変更
        private void changeMovie1(object sender, RoutedEventArgs e)
        {
            movie1.Source = new Uri(baseUri, inputMovie1Name.SelectedItem.ToString());
            movie1_index = inputMovie1Name.SelectedIndex;
            movie1name.Text = MediaData.media[movie1_index].tag;
        }

        private void changeMovie2(object sender, RoutedEventArgs e)
        {
            movie2.Source = new Uri(baseUri, inputMovie2Name.SelectedItem.ToString());
            movie2_index = inputMovie2Name.SelectedIndex;
            movie2name.Text = MediaData.media[movie2_index].tag;
        }
         */
        
        // カメラ位置変更
        private void ChangeCamera(object sender, RoutedEventArgs e)
        {
            var camera = (sender as Button).Content.ToString();
            foreach (var value in MediaData.movieList)
            {
                foreach (var valueV in value.viewList)
                {
                    if (valueV.Key == camera)
                    {
                        // ほっておくと裏でも再生を続けるため強制的に一時停止
                        value.currentPlayer.movie.Pause();
                        value.currentPlayer.sb.Pause();

                        // 時間合わせ
                        var tempPosition = value.currentPlayer.movie.Position;
                        valueV.Value.movie.Position = tempPosition;

                        if (valueV.Value.sb.GetCurrentState() == ClockState.Stopped)
                        {
                            valueV.Value.sb.Begin();
                            valueV.Value.sb.Pause();
                        }
                        valueV.Value.sb.SeekAlignedToLastTick(tempPosition);

                        // プレイヤーの裏表を変更
                        valueV.Value.Visibility = Visibility.Visible;
                        value.currentPlayer = valueV.Value;
                    }
                    else
                    {
                        valueV.Value.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        // 別視点カメラの追加
        private void ExCamera(object sender, RoutedEventArgs e)
        {
            var camera = (sender as Button).Content.ToString();
            foreach (var value in MediaData.movieList)
            {
                foreach (var valueV in value.viewList)
                {
                    if (camera == "なし")
                    {
                        value.currentPlayer.movieEx.Visibility = Visibility.Collapsed;
                    }
                    else if (valueV.Key == camera)
                    {
                        // ほっておくと裏でも再生を続けるため強制的に一時停止
                        value.currentPlayer.movie.Pause();
                        value.currentPlayer.sb.Pause();

                        // 動画を追加
                        value.currentPlayer.movieEx.Source = valueV.Value.movie.Source;
                        value.currentPlayer.movieEx.Visibility = Visibility.Visible;
                    }
                }
            }

        }

        // Canvas操作
        // Canvasのクリア
        private void ClearCanvas(object sender, MouseButtonEventArgs e)
        {
            foreach (var value in MediaData.movieList)
            {
                value.currentPlayer.drawArea.canvas.Children.Clear();
                value.currentPlayer.drawArea.drawingRectangle = null;
            }
        }

        // ペン先の変更
        private void ChangeMode(object sender, MouseButtonEventArgs e)
        {
            foreach (var value in MediaData.movieList)
            {
                switch (value.currentPlayer.drawArea.mode)
                {
                    case AdaptMediaPlayer.Controls.DrawCanvas.Mode.Pen:
                        value.currentPlayer.drawArea.mode
                            = AdaptMediaPlayer.Controls.DrawCanvas.Mode.Rectangle;
                        break;
                    case AdaptMediaPlayer.Controls.DrawCanvas.Mode.Rectangle:
                        value.currentPlayer.drawArea.mode
                            = AdaptMediaPlayer.Controls.DrawCanvas.Mode.Pen;
                        break;
                }
            }
        }

        // 文字列からColorを生成
        private Color TransColor(string strColor)
        {
            var aryColor = strColor.ToCharArray();
            var argb = new List<byte>();

            // #なしだと透明（エラー）
            if (aryColor[0] != '#')
            {
                return Colors.Transparent;
            }

            // 8文字まで認識
            for (var i = 1; i < 9; i += 2)
            {
                argb.Add(Convert.ToByte(aryColor[i].ToString() + aryColor[i + 1].ToString(), 16));
            }

            return Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);
        }

        // タグによる動画群検索（実験中）
        //private void searchView_Click(object sender, MouseButtonEventArgs e)
        //{
        //    if (searchView.Visibility == Visibility.Collapsed)
        //    {
        //        searchView.show();
        //    }
        //    else
        //    {
        //        searchView.hide();
        //    }
        //}
    }
}