using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace nametag_tool
{
    /// <summary>
    /// Interaction logic for ImageOverlayer.xaml
    /// </summary>
    public partial class ImageOverlayer : UserControl
    {
        // hidden internal 
        private Point startDrag;

        private Point startMoveDrag;

        // Font properties
        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ImageOverlayer), new PropertyMetadata("Placeholder"));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public static DependencyProperty TextFontColorProperty = DependencyProperty.Register("TextFontColor", typeof(SolidColorBrush), typeof(ImageOverlayer), new PropertyMetadata(new SolidColorBrush(Colors.Black)));
        public SolidColorBrush TextFontColor
        {
            get { return (SolidColorBrush)GetValue(TextFontColorProperty); }
            set { this.SetValue(TextFontColorProperty, value); }
        }

        public static DependencyProperty TextFontSizeProperty = DependencyProperty.Register("TextFontSize", typeof(double), typeof(ImageOverlayer), new PropertyMetadata(20.00));
        public double TextFontSize
        {
            get { return (double)GetValue(TextFontSizeProperty); }
            set { this.SetValue(TextFontSizeProperty, value); }
        }

        public static DependencyProperty TextFontFamilyProperty = DependencyProperty.Register("TextFontFamily", typeof(FontFamily), typeof(ImageOverlayer), new PropertyMetadata(new FontFamily("Century Gothic")));
        public FontFamily TextFontFamily
        {
            get { return (FontFamily)GetValue(TextFontFamilyProperty); }
            set { this.SetValue(TextFontFamilyProperty, value); }
        }


        //  Image properties
        public static DependencyProperty BackgroundImageProperty = DependencyProperty.Register("BackgroundImage", typeof(ImageSource), typeof(ImageOverlayer), new PropertyMetadata());
        public ImageSource BackgroundImage
        {
            get { return (ImageSource)GetValue(BackgroundImageProperty); }
            set { this.SetValue(BackgroundImageProperty, value); }
        }


        // SelectionArea properties
        public static DependencyProperty MarchingAntsColorProperty = DependencyProperty.Register("MarchingAntsColor", typeof(SolidColorBrush), typeof(ImageOverlayer), new PropertyMetadata(new SolidColorBrush(Colors.Black)));
        public SolidColorBrush MarchingAntsColor
        {
            get { return (SolidColorBrush)GetValue(MarchingAntsColorProperty); }
            set { this.SetValue(MarchingAntsColorProperty, value); }
        }

        public static DependencyProperty MarchingAntsWeightProperty = DependencyProperty.Register("MarchingAntsWeight", typeof(double), typeof(ImageOverlayer), new PropertyMetadata(1.00));
        public double MarchingAntsWeight
        {
            get { return (double)GetValue(MarchingAntsWeightProperty); }
            set { this.SetValue(MarchingAntsWeightProperty, value); }
        }

        public static DependencyProperty SelectionAreaBcgColorProperty = DependencyProperty.Register("SelectionAreaBcgColor", typeof(SolidColorBrush), typeof(ImageOverlayer), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(40, 0, 0, 0))));
        public SolidColorBrush SelectionAreaBcgColor
        {
            get { return (SolidColorBrush)GetValue(SelectionAreaBcgColorProperty); }
            set { this.SetValue(SelectionAreaBcgColorProperty, value); }
        }

        // Control property
        public static DependencyProperty InSelectionModeProperty = DependencyProperty.Register("InSelectioNMode", typeof(bool), typeof(ImageOverlayer), new PropertyMetadata(true));
        public bool InSelectionMode
        {
            get { return (bool)GetValue(InSelectionModeProperty); }
            set { this.SetValue(InSelectionModeProperty, value); }
        }

        // IsSelectionAreaVisible
        public static DependencyProperty IsSelectionAreaVisibleProperty = DependencyProperty.Register("IsSelectionAreaVisible", typeof(Visibility), typeof(ImageOverlayer), new PropertyMetadata(Visibility.Hidden));
        public Visibility IsSelectionAreaVisible
        {
            get { return (Visibility)GetValue(IsSelectionAreaVisibleProperty); }
            set { this.SetValue(IsSelectionAreaVisibleProperty, value); }
        }

        public static DependencyProperty TextFontLineHeightProperty = DependencyProperty.Register("TextFontLineHeight", typeof(double), typeof(ImageOverlayer), new PropertyMetadata(12.0));
        public double TextFontLineHeight
        {
            get { return (double)GetValue(TextFontLineHeightProperty); }
            set { this.SetValue(TextFontLineHeightProperty, value); }
        }

        /// <summary>
        /// Initializes a new instance of the ImageOverlayerControl.
        /// </summary>
        public ImageOverlayer()
        {
            InitializeComponent();

            // add canvas control handlers
            addCanvasControlHandlers();

            setDefaults();

            // add eventhandler
            RectangleControl.MouseEnter += RectangleControl_MouseEnter;
            RectangleControl.MouseLeave += RectangleControl_MouseLeave;
            RectangleControl.MouseDown += RectangleControl_MouseDown;
            RectangleControl.MouseMove += RectangleControl_MouseMove;
            RectangleControl.MouseUp += RectangleControl_MouseUp;
        }

        public void RectangleControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (RectangleControl.IsMouseCaptured)
                RectangleControl.ReleaseMouseCapture();

            updateSelectionArea();
            RectangleControl.Visibility = Visibility.Visible;

            Canvas.SetZIndex(RectangleControl, CanvasControl.Children.Count);
            Canvas.SetZIndex(TextContainer, CanvasControl.Children.Count - 1);
            Canvas.SetZIndex(ImageControl, 0);

            MessageBox.Show($"{RectangleControl.Height}, {RectangleControl.Width}");
            MessageBox.Show($"{Canvas.GetTop(RectangleControl)}, {Canvas.GetLeft(RectangleControl)}");
        }

        public void RectangleControl_MouseMove(object sender, MouseEventArgs e)
        {

            if (RectangleControl.IsMouseCaptured)
            {
                Point currentPoint = e.GetPosition(RectangleControl);

                /// get original top,left position
                var top = Canvas.GetTop(RectangleControl);
                var left = Canvas.GetLeft(RectangleControl);

                // calculate the offset of original startMoveDrag to current mousepos, then add to top,left

                if (RectangleControl.Visibility == Visibility.Hidden)
                    RectangleControl.Visibility = Visibility.Visible;

                // get the difference
                var offsetTop = top + (startMoveDrag.Y - currentPoint.Y);
                var offsetLeft = left + (startMoveDrag.X - currentPoint.X);
                // MessageBox.Show($"{offsetLeft}, {offsetTop}");

                //Move the rectangle to proper place
                RectangleControl.RenderTransform = new TranslateTransform(offsetLeft, offsetTop);
            }
        }

        public void RectangleControl_MouseDown(object sender, MouseEventArgs e)
        {
            // get x,y position of top,left of rectangle
            //Set the start point
            // startMoveDrag = e.GetPosition(CanvasControl);
            var rectTop = RectangleControl.RenderTransform.Value.OffsetY;
            var rectLeft = RectangleControl.RenderTransform.Value.OffsetX;

            startMoveDrag = new Point(rectLeft, rectTop);

            //Capture the mouse
            if (!RectangleControl.IsMouseCaptured)
                RectangleControl.CaptureMouse();

        }

        public void RectangleControl_MouseLeave(object sender, MouseEventArgs e)
        {
            CanvasControl.Cursor = Cursors.Cross;
        }

        public void RectangleControl_MouseEnter(object sender, MouseEventArgs e)
        {
            // set mouse cursor to move icon
            CanvasControl.Cursor = Cursors.SizeAll;
        }

        public void addCanvasControlHandlers()
        {
            CanvasControl.MouseDown += CanvasControl_MouseDown;
            CanvasControl.MouseUp += CanvasControl_MouseUp;
            CanvasControl.MouseMove += CanvasControl_MouseMove;
        }
        public void removeCanvasControlHandlers()
        {
            CanvasControl.MouseDown -= CanvasControl_MouseDown;
            CanvasControl.MouseUp -= CanvasControl_MouseUp;
            CanvasControl.MouseMove -= CanvasControl_MouseMove;
        }

        private void CanvasControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!InSelectionMode) return;

            // check if it was an event from the rectangle child
            if (!(e.OriginalSource is Rectangle))
            {
                var bckSaveOriginalPos = startDrag;

                //Set the start point
                startDrag = e.GetPosition(CanvasControl);
                //Move the selection marquee on top of all other objects in canvas
                Canvas.SetZIndex(RectangleControl, CanvasControl.Children.Count);
                Canvas.SetZIndex(TextContainer, CanvasControl.Children.Count - 1);
                Canvas.SetZIndex(ImageControl, 0);

                //Capture the mouse
                if (!CanvasControl.IsMouseCaptured)
                    CanvasControl.CaptureMouse();
            }

        }

        public void setDefaults()
        {
            textTest.TextWrapping = TextWrapping.Wrap;
            textTest.TextAlignment = TextAlignment.Center;
            textTest.HorizontalAlignment = HorizontalAlignment.Stretch;
            textTest.VerticalAlignment = VerticalAlignment.Center;
        }

        public void updateSelectionArea()
        {
            //Release the mouse
            if (CanvasControl.IsMouseCaptured)
                CanvasControl.ReleaseMouseCapture();
            // CanvasControl.Cursor = Cursors.Arrow;

            // canvas.UpdateLayout();
            var left = RectangleControl.RenderTransform.Value.OffsetX;
            var top = RectangleControl.RenderTransform.Value.OffsetY;

            var rw = RectangleControl.Width;
            var rh = RectangleControl.Height;

            // var xCenter = left + rw / 2;
            // var yCenter = top + rh / 2;

            TextContainer.Width = rw;
            TextContainer.Height = rh;

            textTest.FontWeight = FontWeight.FromOpenTypeWeight(700);

            // set text to visible
            textTest.Visibility = Visibility.Visible;

            TextContainer.RenderTransform = new TranslateTransform(left, top);
        }

        private void CanvasControl_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (!InSelectionMode) return;

            if (!(e.OriginalSource is Rectangle))
            {
                updateSelectionArea();
            }

        }

        private void CanvasControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!InSelectionMode) return;
            if (!(e.OriginalSource is Rectangle))
            {
                if (CanvasControl.IsMouseCaptured)
                {
                    Point currentPoint = e.GetPosition(CanvasControl);

                    //Calculate the top left corner of the rectangle 
                    //regardless of drag direction
                    double x = startDrag.X < currentPoint.X ? startDrag.X : currentPoint.X;
                    double y = startDrag.Y < currentPoint.Y ? startDrag.Y : currentPoint.Y;

                    if (RectangleControl.Visibility == Visibility.Hidden)
                        RectangleControl.Visibility = Visibility.Visible;

                    HitTestResult result = VisualTreeHelper.HitTest(ImageControl, currentPoint);
                    if (result == null)
                    {
                        RectangleControl.Visibility = Visibility.Hidden;
                        textTest.Text = "";
                        return;
                    }

                    //Move the rectangle to proper place
                    RectangleControl.RenderTransform = new TranslateTransform(x, y);
                    //Set its size
                    RectangleControl.Width = Math.Abs(e.GetPosition(CanvasControl).X - startDrag.X);
                    RectangleControl.Height = Math.Abs(e.GetPosition(CanvasControl).Y - startDrag.Y);
                }
            }
        }

        private void CanvasControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!(e.OriginalSource is Rectangle))
            {
                if (this.InSelectionMode)
                {
                    // show cross  
                    CanvasControl.Cursor = Cursors.Cross;
                }
                else
                {
                    CanvasControl.Cursor = Cursors.Arrow;
                }
            }
        }
    }
}
