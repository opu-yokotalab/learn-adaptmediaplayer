using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;

namespace AdaptMediaPlayer.Controls
{
    // 注：ここのコードはほぼパクリです．
    // 対外公開の際には注意されたし．
    public partial class DrawCanvas : UserControl
    {
        // 演出オーサリングの変数
        private bool dragStart = false;
        private Point dragStartPnt;
        public Rectangle drawingRectangle;
        public Mode mode = Mode.Pen;

        public enum Mode
        {
            Pen,
            Rectangle
        }

        public DrawCanvas()
        {
            InitializeComponent();
        }
        // 演出作成系
        private void canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            StopDragging();
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopDragging();
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartDragging(e.GetPosition(canvas));
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragStart)
            {
                var curPos = e.GetPosition(canvas);
                curPos = new Point(curPos.X,curPos.Y);

                switch (mode)
                {
                    case (Mode.Pen):
                        DrawLine(dragStartPnt, curPos);
                        dragStartPnt = curPos;
                        break;
                    case (Mode.Rectangle):
                        DrawRectangle(dragStartPnt, curPos);
                        break;
                }
            }
        }

        private void StartDragging(Point startPnt)
        {
            if (!dragStart)
            {
                dragStart = true;
                dragStartPnt = startPnt;
            }
        }

        private void StopDragging()
        {
            if (dragStart)
            {
                dragStart = false;
            }
        }

        private void DrawLine(Point startPnt, Point endPnt)
        {
            var line = DrawLine(
                startPnt.X, endPnt.X, startPnt.Y, endPnt.Y
            );

        }
        public Line DrawLine(double x1, double x2, double y1, double y2)
        {
            var line = new Line
            {
                X1 = x1,
                X2 = x2,
                Y1 = y1,
                Y2 = y2,
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 2,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round
            };

            canvas.Children.Add(line);

            return line;
        }

        private void DrawRectangle(Point startPnt, Point endPnt)
        {
            var width = endPnt.X - startPnt.X;
            var height = endPnt.Y - startPnt.Y;

            if (drawingRectangle == null)
            {
                drawingRectangle = DrawRectangle(startPnt.X, startPnt.Y, width, height);
            }

            if (width < 0)
            {
                Canvas.SetLeft(drawingRectangle, startPnt.X + width);
            }
            else
            {
                Canvas.SetLeft(drawingRectangle, startPnt.X);
            }
            if (height < 0)
            {
                Canvas.SetTop(drawingRectangle, startPnt.Y + height);
            }
            else
            {
                Canvas.SetTop(drawingRectangle, startPnt.Y);
            }

            drawingRectangle.Width = Math.Abs(width);
            drawingRectangle.Height = Math.Abs(height);
        }

        public Rectangle DrawRectangle(double left, double top, double width, double height)
        {
            var rectangle = new Rectangle
            {
                Width = Math.Abs(width),
                Height = Math.Abs(height),
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 2,
                Name = "hoge"
            };
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);

            canvas.Children.Add(rectangle);

            return rectangle;
        }
    }
}
