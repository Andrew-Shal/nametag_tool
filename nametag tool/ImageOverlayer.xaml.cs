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
        private Point startMoveDrag2;

        private bool invalid_bounds = false;

        // Font properties
        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ImageOverlayer), new PropertyMetadata("[Enter Text]"));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set {
                if(value.Trim() == "")
                {
                    this.SetValue(TextProperty,"[Enter Text]");
                    return;
                }
                this.SetValue(TextProperty, value); 
            
            }
        }

        public static DependencyProperty TextFontColorProperty = DependencyProperty.Register("TextFontColor", typeof(SolidColorBrush), typeof(ImageOverlayer), new PropertyMetadata(new SolidColorBrush(Colors.Black)));
        public SolidColorBrush TextFontColor
        {
            get { return (SolidColorBrush)GetValue(TextFontColorProperty); }
            set { this.SetValue(TextFontColorProperty, value); }
        }

        public static DependencyProperty TextFontSizeProperty = DependencyProperty.Register("TextFontSize", typeof(double), typeof(ImageOverlayer), new PropertyMetadata(80.00));
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
                //Set the start point
                startDrag = e.GetPosition(CanvasControl);
                //Move the selection marquee on top of all other objects in canvas
                Canvas.SetZIndex(RectangleControl, CanvasControl.Children.Count);

                //Capture the mouse
                if (!CanvasControl.IsMouseCaptured)
                    CanvasControl.CaptureMouse();
            }
            if(e.OriginalSource is Rectangle){
                startMoveDrag = e.GetPosition(RectangleControl);
                startMoveDrag2 = e.GetPosition(CanvasControl);

                //Capture the mouse
                if (!RectangleControl.IsMouseCaptured)
                    RectangleControl.CaptureMouse();
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

            if (RectangleControl.IsMouseCaptured)
                RectangleControl.ReleaseMouseCapture();

            // canvas.UpdateLayout();
            var left = RectangleControl.RenderTransform.Value.OffsetX;
            var top = RectangleControl.RenderTransform.Value.OffsetY;

            var rw = RectangleControl.Width;
            var rh = RectangleControl.Height;

            TextContainer.Width = rw;
            TextContainer.Height = rh;

            textTest.FontWeight = FontWeight.FromOpenTypeWeight(700);

            if (!invalid_bounds)
            {
                // set text to visible
                textTest.Visibility = Visibility.Visible;
            }
            else
            {
                textTest.Visibility = Visibility.Hidden;
                RectangleControl.Visibility = Visibility.Hidden;
            }
            invalid_bounds = false;

            TextContainer.RenderTransform = new TranslateTransform(left, top);
        }

        private void CanvasControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!InSelectionMode) return;

            if (!(e.OriginalSource is Rectangle))
            {
                //Move the selection marquee on top of all other objects in canvas
                Canvas.SetZIndex(RectangleControl, CanvasControl.Children.Count);
                Canvas.SetZIndex(TextContainer, CanvasControl.Children.Count - 1);
                Canvas.SetZIndex(ImageControl, 0);
                updateSelectionArea();
            }
            if ((e.OriginalSource is Rectangle))
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
                        invalid_bounds = true;
                        return;
                    }

                    //Move the rectangle to proper place
                    RectangleControl.RenderTransform = new TranslateTransform(x, y);
                    //Set its size
                    RectangleControl.Width = Math.Abs(e.GetPosition(CanvasControl).X - startDrag.X);
                    RectangleControl.Height = Math.Abs(e.GetPosition(CanvasControl).Y - startDrag.Y);
                }
            }


            if ((e.OriginalSource is Rectangle))
            {
                if (RectangleControl.IsMouseCaptured)
                {
                    Point currentPoint = e.GetPosition(CanvasControl);

                    Point currentPoint2 = e.GetPosition(RectangleControl);

                    //Calculate the top left corner of the rectangle 
                    //regardless of drag direction
                    double x = currentPoint.X - startMoveDrag.X;
                    double y = currentPoint.Y - startMoveDrag.Y;

                    if (RectangleControl.Visibility == Visibility.Hidden)
                        RectangleControl.Visibility = Visibility.Visible;

                    UIElement container = VisualTreeHelper.GetParent(CanvasControl) as UIElement;
                    Point relativeLocationTopLeft = RectangleControl.TranslatePoint(new Point(0, 0), container);

                    double w = VisualTreeHelper.GetContentBounds(RectangleControl).Width;
                    double h = VisualTreeHelper.GetContentBounds(RectangleControl).Height;

                    Point relativeLocationBottomRight = new Point(relativeLocationTopLeft.X + w, relativeLocationTopLeft.Y + h);


                    HitTestResult topLeftTest = VisualTreeHelper.HitTest(ImageControl, relativeLocationTopLeft);
                    HitTestResult bottomRightTest = VisualTreeHelper.HitTest(ImageControl, relativeLocationBottomRight);
                    if (topLeftTest == null || bottomRightTest == null)
                    {
                        // was to set rectangle back to original position
                        var originalX = startMoveDrag2.X - currentPoint2.X;
                        var originalY = startMoveDrag2.Y - currentPoint2.Y;
                        RectangleControl.RenderTransform = new TranslateTransform(originalX, originalY);
                        RectangleControl.ReleaseMouseCapture();
                        return;
                    }

                    //Move the rectangle to new location
                    RectangleControl.RenderTransform = new TranslateTransform(x, y);
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
