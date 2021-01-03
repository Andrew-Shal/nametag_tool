using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.IO;

namespace nametag_tool
{
    /// <summary>
    /// Interaction logic for ProgressBarWindow.xaml
    /// </summary>
    public partial class ProgressBarWindow : Window
    {

        ImageOverlayer _overlayer;
        string _fullFolderPath;
        List<string> _names = new List<string>();

        CancellationTokenSource cts = new CancellationTokenSource();

        string folderToRemove = "";

        public ProgressBarWindow(ImageOverlayer overlayer, string fullFolderPath, List<string> names)
        {
            _overlayer = overlayer;
            _fullFolderPath = fullFolderPath;
            _names = names;
            
            InitializeComponent();
        }

        public async Task init()
        {
            Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
            progress.ProgressChanged += Progress_ProgressChanged;

            CancelOperationButton.IsEnabled = false;

            try
            {
                await RunBatchCreationAsync(progress, cts.Token);
            }
            catch (OperationCanceledException) {
                ProgressInfoLabel.Content = $"Operation Cancelled";
                ViewDetailsListBox.Text += $"Operation Cancelled {Environment.NewLine}";

                try { 
                    if (folderToRemove.Length > 1) {
                        folderToRemove = System.IO.Path.GetDirectoryName(folderToRemove);
                        if (Directory.Exists(folderToRemove))
                        {
                            Directory.Delete(folderToRemove, true);

                            // reset folder to delete
                            folderToRemove = "";
                        }
                    }
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }

        private void Progress_ProgressChanged(object sender, ProgressReportModel e)
        {
            // update ui
            ProgressBarControl.Value = e.PercentageComplete;
            ProgressPercentageLabel.Content = $"{e.PercentageComplete}%";
            
            foreach(var i in e.Items)
            {
                ViewDetailsListBox.Text += $"created {i.FilePath} {Environment.NewLine}";
                ProgressInfoLabel.Content = $"created {i.FilePath}";

                if (e.PercentageComplete == 100)
                {
                    ProgressInfoLabel.Content = "Operation Completed";
                    ViewDetailsListBox.Text += $"Operation Completed {Environment.NewLine}";
                    CancelOperationButton.IsEnabled = false;
                }
            }
        }

        private async Task RunBatchCreationAsync(IProgress<ProgressReportModel> progress,CancellationToken cancellationToken)
        {
            CancelOperationButton.IsEnabled = true;

            List<bool> output = new List<bool>();
            
            foreach (string name in _names)
            {
                _overlayer.Text = name;
                _overlayer.RectangleControl.Visibility = Visibility.Hidden;
                _overlayer.CanvasControl.UpdateLayout();
                _overlayer.updateSelectionArea();

                // get name of what is in placeholderTextInput
                var fullFilePath = _fullFolderPath + @"\" + name + ".png";

                // set to not overwrite files by default, could change in the future. message box prompts that file already exists, ask if 
                // user wants to overwrite or rename, and if they want to do for all the rest. use a flag, should overwrite
                fullFilePath = fileNameIfExists(fullFilePath, name, 1);

                bool results = await ExportToPng(new Uri(fullFilePath), _overlayer.CanvasControl);
                output.Add(results);

                if (cancellationToken.IsCancellationRequested)
                {
                    // clean up - remove created folder/images
                    folderToRemove = fullFilePath;

                    CancelOperationButton.IsEnabled = false;
                }

                cancellationToken.ThrowIfCancellationRequested();

                ProgressReportModel prm = new ProgressReportModel();
                prm.PercentageComplete = (output.Count * 100) / _names.Count;
                prm.Items.Add(new ItemDataModel(name, fullFilePath));
                progress.Report(prm);
            }
        }

        private void ViewDetailsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ViewDetailsListBox.Visibility = Visibility.Visible;
            this.Height = MaxHeight;
        }

        private void ViewDetailsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            ViewDetailsListBox.Visibility = Visibility.Hidden;
            this.Height = this.MinHeight;
        }

        private async Task<bool> ExportToPng(Uri path, Canvas surface)
        {
            if (path == null) return false;
            try
            {
                    await Task.Run(() => {
                    this.Dispatcher.Invoke(() =>{

                        // Save current canvas transform
                        Transform transform = surface.LayoutTransform;
                        // reset current transform (in case it is scaled or rotated)
                        surface.LayoutTransform = null;

                        // Get the size of the canvas

                        // Instead of canvas size, we can get the image size
                        Size size = new Size(surface.ActualWidth, surface.ActualHeight);
                        //Size size = new Size(surface.ImageControl.ActualWidth,surface.ImageControl.ActualHeight);

                        // Measure and arrange the surface
                        // very important
                        surface.Measure(size);
                        surface.Arrange(new Rect(size));

                        // Create a rendeer bitmap and push the surface to it
                        RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Pbgra32);
                        renderBitmap.Render(surface);

                        // Create a file stream for saving image
                        using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create))
                        {
                            // Use png encoder for our data
                            PngBitmapEncoder encoder = new PngBitmapEncoder();
                            // push the rendered bitmap to it
                            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                            // save the data to the stream
                            encoder.Save(outStream);
                        }

                        // Restore previously saved layout
                        surface.LayoutTransform = transform;
                    });
                });
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
        }
        private string fileNameIfExists(string pathToFile, string fileName, int currentCount)
        {
            if (File.Exists(pathToFile))
            {
                var fName = fileName;
                string temp = _fullFolderPath + @"\" + fName + $"({currentCount})" + ".png";
                int count = currentCount + 1;
                return fileNameIfExists(temp, fileName, count);
            }
            return pathToFile;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // async driver
            await init();
        }

        private void CancelOperationButton_Click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cts.Cancel();
        }
    }
}
