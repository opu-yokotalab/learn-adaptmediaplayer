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
// 追加
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace AdaptMediaPlayer.Controls
{
    public partial class MediaPlayer : UserControl
    {
        public int movie_index;
        public MediaData.Media media;
        public Storyboard sb;
        
        //public double currentPlayRatio;
        //public int reachMarker = 1;
        //public List<double> slowPlayRatio = new List<double>();
        
        public MediaPlayer()
        {
            InitializeComponent();
        }

        // MediaOpenedのキック時
        // 開かれた動画にマーカーを付ける
        private void movie_MediaOpened(object sender, RoutedEventArgs e)
        {
            // 動画の読み込み時にスライダ操作
            Volume.Value = 1.0;

            // 前の動画のマーカーをクリア
            movie.Markers.Clear();
            markers.Children.Clear();
            
            var i = 0;
            foreach (var value in MediaData.media[movie_index].sync)
            {
                var valueT = TimeSpan.Parse(value.time);
                if (movie.NaturalDuration.TimeSpan > valueT)
                {
                    // TimelineMarkerの定義
                    var TLMarker = new TimelineMarker
                    {
                        Time = valueT,
                        Text = "enabled"
                    };
                    movie.Markers.Add(TLMarker);

                    // 同期ポイントボタンの定義
                    var image = new Image
                    {
                        Source = new BitmapImage(new Uri("../image/marker.png", UriKind.Relative)),
                        Margin = new Thickness(-10, 0, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Height = 20,
                        Width = 20,
                        Tag = i
                    };
                    Canvas.SetLeft(image, valueT.TotalSeconds / movie.NaturalDuration.TimeSpan.TotalSeconds * 320);
                    Grid.SetColumn(image, 0);
                    Grid.SetRow(image, 3);
                    Grid.SetColumnSpan(image, 3);

                    // 同期ポイントボタンを押したら
                    // 対応するmovie1とmovie2の同期ポイントに同期
                    image.MouseLeftButtonDown += new MouseButtonEventHandler((object _s, MouseButtonEventArgs _e) =>
                    {
                        foreach (var valueL in MediaData.movieList)
                        {
                            valueL.currentPlayer.movie.Position = valueL.currentPlayer.movieEx.Position
                                = TimeSpan.Parse(MediaData.media[valueL.currentPlayer.movie_index].sync[(int)image.Tag].time);

                            if (valueL.currentPlayer.sb.GetCurrentState() == ClockState.Stopped)
                            {
                                valueL.currentPlayer.sb.Begin();
                                valueL.currentPlayer.sb.Pause();
                            }
                            valueL.currentPlayer.sb.SeekAlignedToLastTick(valueL.currentPlayer.movie.Position);
                        }
                    });
                    image.MouseWheel += new MouseWheelEventHandler((object _s, MouseWheelEventArgs _e) =>
                    {
                        switch (movie.Markers[(int)image.Tag].Text)
                        {
                            case "enabled":
                                foreach (var valueL in MediaData.movieList)
                                {
                                    (valueL.currentPlayer.markers.Children[(int)image.Tag] as Image).Source =
                                        new BitmapImage(new Uri("../image/marker_disabled.png", UriKind.Relative));
                                    valueL.currentPlayer.movie.Markers[(int)image.Tag].Text = "disabled";
                                }
                                break;
                            //case "disabled":
                            //    (movie1Markers.Children[(int)image.Tag] as Image).Source =
                            //        new BitmapImage(new Uri("image/marker.png", UriKind.Relative));
                            //    movie1.Markers[(int)image.Tag].Text = "enabled";
                            //    (movie2Markers.Children[(int)image.Tag] as Image).Source =
                            //        new BitmapImage(new Uri("image/marker.png", UriKind.Relative));
                            //    movie2.Markers[(int)image.Tag].Text = "enabled";
                            //    break;
                        }
                    });
                    markers.Children.Add(image);
                    i++;
                }
            }

        }

        // MarkerReachedのキック時
        // movie1到達で一時停止，movie2到達で再生
        private void movie_MarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
        {
            //if (!MediaData.IsSlowPlay)
            //{
            if (e.Marker.Text != "disabled")
            {
                foreach (var value in MediaData.movieList)
                {
                    if (value.currentPlayer.movie != movie)
                    {
                        if (value.currentPlayer.movie.CurrentState == MediaElementState.Playing)
                        {
                            PauseMedia(sender, e);
                        }
                        else if (value.currentPlayer.movie.CurrentState == MediaElementState.Paused)
                        {
                            value.currentPlayer.PlayMedia(sender, e);
                        }
                    }
                }
            }
            //}
            //else
            //{
            //    //movie.Pause();
            //    MediaData.movieList[0].currentPlayer.currentPlayRatio
            //        = MediaData.movieList[0].currentPlayer.slowPlayRatio[reachMarker];
            //    reachMarker++;
            //    //if (currentPlayRatio == 1.0)
            //    //{
            //    //    movie.Play();
            //    //}
            //    //Debug.WriteLine(currentPlayRatio);
            //}
        }

        public void StopMedia(object sender, RoutedEventArgs e)
        {
            movie.Stop();
            movieEx.Stop();

            sb.SeekAlignedToLastTick(TimeSpan.Zero);
            sb.Stop();
        }
        public void PauseMedia(object sender, RoutedEventArgs e)
        {
            movie.Pause();
            movieEx.Pause();

            sb.Pause();
        }
        public void PlayMedia(object sender, RoutedEventArgs e)
        {
            movie.Play();
            movieEx.Play();

            if (sb.GetCurrentState() == ClockState.Stopped)
            {
                sb.Begin();
            }
            else
            {
                sb.Resume();
            }
        }

        // シークバー上をクリックして移動
        private void progressBarBg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(progressBarBg);
            var second = movie.NaturalDuration.TimeSpan.TotalSeconds * (position.X / progressBarBg.Width);

            movie.Position = movieEx.Position = TimeSpan.FromSeconds(second);

            if (sb.GetCurrentState() == ClockState.Stopped)
            {
                sb.Begin();
                sb.Pause();
            }
            sb.SeekAlignedToLastTick(movie.Position);
        }

        // 音量変更
        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            movie.Volume = (sender as Slider).Value;
        }

        private void movieEx_MediaOpened(object sender, RoutedEventArgs e)
        {
            movieEx.Position = movie.Position;
        }
    }
}
