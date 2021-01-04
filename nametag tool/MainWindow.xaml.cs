using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using Xceed.Wpf.Toolkit;

namespace nametag_tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 
        public ICommand OnRemoveBtnClickCommand { get; set; }
        public ICommand OnPreviewBtnClickCommand { get; set; }
        public ICommand OnUpdateBtnClickCommand { get; set; }
        public ICommand OnEditBtnClickCommand { get; set; }
        public ICommand OnCancelBtnClickCommand { get; set; }

        //
        ObservableCollection<NameDataModel> names = new ObservableCollection<NameDataModel>();

        // 
        public string exportImagesPath = "";

        // will append dynamic dates to folder outputs
        public const string DEFAULT_DIR_NAME = "nametag_exports - ";

        public MainWindow()
        {
            InitializeComponent();

            setActionCommands();

            sortByCbx.ItemsSource = Enum.GetValues(typeof(OrderBy));

            HotizontalTextAlignmentCbx.ItemsSource = Enum.GetValues(typeof(HorizontalAlignment));
            VeticalTextAlignmentCbx.ItemsSource = Enum.GetValues(typeof(VerticalAlignment));

            names.CollectionChanged += Names_CollectionChanged;

            // set defaults
            FontFamilyCombo.SelectedIndex = 0;
            FontSizeCombo.Value = 60.0;
            VeticalTextAlignmentCbx.SelectedItem = VerticalAlignment.Center;
            HotizontalTextAlignmentCbx.SelectedItem = HorizontalAlignment.Center;
            backgroundColorPickerControl.SelectedColor = Colors.Black;
            zoomSlider.Value = .26; // hard code optimal size for default image in image preview control
        }

        public void setActionCommands() { 
            OnRemoveBtnClickCommand = new ActionCommand(x => {
                NameDataModel nameModel = x as NameDataModel;
                names.Remove(nameModel); 
                /* clear text that is in preview if it was the removed item*/ 
                if (nameModel.Name.ToString() == placeholderTextInput.Text) {
                    placeholderTextInput.Clear(); 
                } 
            });

            OnPreviewBtnClickCommand = new ActionCommand(x => {
                NameDataModel nameModel = x as NameDataModel;
                if (!overlayer.textTest.IsVisible) { 
                    System.Windows.MessageBox.Show("Select an area on the preview canvas");
                    return; 
                } 
                overlayer.Text = nameModel.Name.ToString(); 
                placeholderTextInput.Text = nameModel.Name.ToString(); 
                csvNamesListBox.SelectedItem = nameModel; 
            });

            OnEditBtnClickCommand = new ActionCommand(x => {
                // we want to show the input box and update button for this item
                NameDataModel nameModel = x as NameDataModel;
                // Getting idx of listbox item currently selected
                var idx = csvNamesListBox.Items.IndexOf(nameModel);
                ListBoxItem lbiAtIdx = csvNamesListBox.ItemContainerGenerator.ContainerFromIndex(idx) as ListBoxItem;
                // Getting the ContentPresenter of ListBoxItem at Index
                ContentPresenter contentPresenter = Utility.FindVisualChild<ContentPresenter>(lbiAtIdx);
                // Finding editBoxGroup from the DataTemplate that is set on that ContentPresenter
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                
                Grid editBoxGroup = (Grid)dataTemplate.FindName("editBoxGroup", contentPresenter);
                Button editBtn = (Button)dataTemplate.FindName("toggleEditContainerBtn", contentPresenter);
                TextBox editTextBox = (TextBox)dataTemplate.FindName("textBox", contentPresenter);

                // set text box to name value
                editTextBox.Text = nameModel.Name;

                // set to false, since we are already editing
                editBtn.IsEnabled = false;

                // show it
                editBoxGroup.Visibility = Visibility.Visible;
            });

            OnCancelBtnClickCommand = new ActionCommand(x =>
            {
                NameDataModel nameModel = x as NameDataModel;
                // Getting idx of listbox item currently selected
                var idx = csvNamesListBox.Items.IndexOf(nameModel);
                ListBoxItem lbiAtIdx = csvNamesListBox.ItemContainerGenerator.ContainerFromIndex(idx) as ListBoxItem;
                // Getting the ContentPresenter of ListBoxItem at Index
                ContentPresenter contentPresenter = Utility.FindVisualChild<ContentPresenter>(lbiAtIdx);
                // Finding textbox from the DataTemplate that is set on that ContentPresenter
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
               
                Grid editBoxGroup = (Grid)dataTemplate.FindName("editBoxGroup", contentPresenter);
                Button editBtn = (Button)dataTemplate.FindName("toggleEditContainerBtn", contentPresenter);

                // re-enable edit button and hide edit container
                editBtn.IsEnabled = true;
                editBoxGroup.Visibility = Visibility.Hidden;
            });

            OnUpdateBtnClickCommand = new ActionCommand(x => {
                NameDataModel nameModel = x as NameDataModel;
                // Getting idx of listbox item currently selected
                var idx = csvNamesListBox.Items.IndexOf(nameModel);
                ListBoxItem lbiAtIdx = csvNamesListBox.ItemContainerGenerator.ContainerFromIndex(idx) as ListBoxItem;
                // Getting the ContentPresenter of ListBoxItem at Index
                ContentPresenter contentPresenter = Utility.FindVisualChild<ContentPresenter>(lbiAtIdx);
                // Finding textbox from the DataTemplate that is set on that ContentPresenter
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                
                Grid editBoxGroup = (Grid)dataTemplate.FindName("editBoxGroup", contentPresenter);
                Button editBtn = (Button)dataTemplate.FindName("toggleEditContainerBtn", contentPresenter);
                TextBox editTextBox = (TextBox)dataTemplate.FindName("textBox", contentPresenter);

                // re-enable edit button and hide edit container
                editBtn.IsEnabled = true;
                editBoxGroup.Visibility = Visibility.Hidden;

                // getting idx of current item being looked at in the actual data model
                int nameDataModelIdx = names.IndexOf(nameModel);

                // update the data model entry with new name model instance, copying same values from old instance
                names[nameDataModelIdx] = new NameDataModel(nameModel.Id, editTextBox.Text.Trim());

                #region defferred functionality
                /*
                var t = "";
                foreach (NameDataModel na in names) {
                    t += $"{na.Name}, {Environment.NewLine}";
                }
                System.Windows.MessageBox.Show(t);
                */
                /*
                 * Defferred, binding the name in xaml updates the data source item
                 * 
                // getting idx of current item being looked at in the actual data model
                int nameDataModelIdx = names.IndexOf(nameModel);

                // update the name for the current data model
                nameModel.Name = myTextBox.Text.Trim();

                // update the data model entry where they match
                names[nameDataModelIdx] = nameModel;
                */
                #endregion
            });
        }


        private void Names_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // disable batch creation button if names is not populated
            bool shouldBeEnabled = names.Count < 1 ? false : true;
            StartBatchCreationBtn.IsEnabled = sortByCbx.IsEnabled = ExportCurrentCsvBtn.IsEnabled = shouldBeEnabled;
        }

        private void selectCsvBtn_Click(object sender, RoutedEventArgs e)
        {
            // check if names list is not already populated
            if(names.Count > 1)
            {
                MessageBoxResult res = System.Windows.MessageBox.Show("Are you sure you want to select a new CSV file? Note that this will clear the current list", "Clear List",MessageBoxButton.YesNo);

                if (res != MessageBoxResult.Yes) return;
               
               // clear names
                names.Clear();
            }

            // open selection dialog
            OpenFileDialog selectCsvDialog = new OpenFileDialog();
            selectCsvDialog.Multiselect = false;
            selectCsvDialog.Filter = "Comma Separated|*.txt";

            if (selectCsvDialog.ShowDialog() != true) return;

            // parse csv
            var fileContent = File.ReadAllText(selectCsvDialog.FileName);
            string[] namesArr = parseFile(fileContent, ',');

            buildNamesModel(namesArr);
            updateNameCount(names.Count,0);

            // set datasource for view listbox
            csvNamesListBox.ItemsSource = names;
        }

        public void buildNamesModel(string[] namesArr)
        {
            for (int i = 0; i < namesArr.Length; i++) names.Add(new NameDataModel(i, namesArr[i]));
        }

        public void updateNameCount(int fileLoadedCount, int nameAddedCount)
        {
            var output = "";
            output += fileLoadedCount > 1 ? $"{ fileLoadedCount} - csv file | " : "";
            output += nameAddedCount > 1  ? $"{nameAddedCount} - manually added | " : "";
            output += fileLoadedCount > 1 && nameAddedCount > 1 ? $"{fileLoadedCount + nameAddedCount} - Total" : "";
            namesFoundCountLbl.Content = output;
        }

        public string[] parseFile(string content, char parseBy) {
            return content.Split(parseBy).Where(i => i.Trim().Length > 1).Select(j => j.Trim()).ToArray();
        }

        private void selectPlaceholderBtn_Click(object sender, RoutedEventArgs e)
        {
            // open image selection dialog
            OpenFileDialog selectImageDialog = new OpenFileDialog();
            selectImageDialog.Multiselect = false;
            selectImageDialog.Filter = "Image Files|*.png;*jpg;*.jpep";

            if (selectImageDialog.ShowDialog() == true) {
                var filePath = selectImageDialog.FileName;
                var converter = new ImageSourceConverter();

                overlayer.BackgroundImage = (ImageSource)converter.ConvertFromInvariantString(filePath);

                overlayer.CanvasControl.UpdateLayout();
                overlayer.updateSelectionArea();

                // height or width is not needed since it is being mapped to a silder with a single value
                double optimalZoom = Dimension.calculateOptimalZoom(
                    new Dimension(
                        overlayer.ImageControl.ActualWidth,
                        overlayer.ImageControl.ActualHeight),
                    new Dimension(
                        ImagePreviewBoxScrollContainer.ActualWidth,
                        ImagePreviewBoxScrollContainer.ActualHeight)
                    );

                // add 1 percent padding on either 
                zoomSlider.Value = optimalZoom != 1d ? (optimalZoom - .01d) : optimalZoom;
            }
        }

        private void placeholderTextInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox elem = (TextBox)sender;
            overlayer.Text = elem.Text;
        }

        private void FontFamilyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox elem = (ComboBox)sender;
            overlayer.TextFontFamily = new FontFamily(elem.SelectedItem.ToString());
            overlayer.CanvasControl.UpdateLayout();
            overlayer.updateSelectionArea();
        }

        private void FontSizeCombo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                DoubleUpDown cb = (DoubleUpDown)sender;
                double fontSize;
                double.TryParse(cb.Text, out fontSize);
                overlayer.TextFontSize = fontSize;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Invalid value entered");
                // set a default when an err thrown
                overlayer.TextFontSize = 12.00;
            }
        }

        private void backgroundColorPickerControl_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if(e.NewValue != e.OldValue) overlayer.TextFontColor = new SolidColorBrush((Color)e.NewValue);
        }

        private void IsSelectionAreaVisibleCbx_Click(object sender, RoutedEventArgs e)
        {
            CheckBox elem = (CheckBox)sender;

            overlayer.RectangleControl.Visibility = (bool)elem.IsChecked ? Visibility.Visible : Visibility.Hidden;
            overlayer.CanvasControl.UpdateLayout();
            overlayer.updateSelectionArea();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;

            OrderBy sortOrder;

            Enum.TryParse<OrderBy>(cb.SelectedValue.ToString(), out sortOrder);
            names = Utility.SortNameCollection(names, sortOrder);
        }

        private void TextLineHeightCbx_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                // update line height
                DoubleUpDown cb = (DoubleUpDown)sender;
                overlayer.TextFontLineHeight = (double)cb.Value;
            }catch(Exception ex)
            {
                // set a default when an err thrown
                overlayer.TextFontLineHeight = 18.0;
            }
        }

        private void HotizontalTextAlignmentCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            HorizontalAlignment ha;

            Enum.TryParse<HorizontalAlignment>(cb.SelectedValue.ToString(), out ha);
            overlayer.textTest.HorizontalAlignment = ha;
        }

        private void VeticalTextAlignmentCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            VerticalAlignment va;

            Enum.TryParse<VerticalAlignment>(cb.SelectedValue.ToString(), out va);
            overlayer.textTest.VerticalAlignment = va;
        }


        private void AddNewNameBtn_Click(object sender, RoutedEventArgs e)
        {
            // empty, ignore 
            if (AddNewNameInp.Text.Trim().Length < 1) return;

            // add new name to names obj
            names.Add(new NameDataModel(names.Count + 1, AddNewNameInp.Text.Trim()));

            // assume they havent added a list
            if(csvNamesListBox.Items.Count < 1)
            {
                // set datasource for view listbox
                csvNamesListBox.ItemsSource = names;
            }

            // select addition
            csvNamesListBox.Focus();
            csvNamesListBox.SelectedValue = AddNewNameInp.Text;
            csvNamesListBox.ScrollIntoView(csvNamesListBox.SelectedValue);
            // might want call sortby to update its position if there was a pre existing list

            // clear add new current text
            AddNewNameInp.Clear();

        }

        private void ExportCurrentCsvBtn_Click(object sender, RoutedEventArgs e)
        {
            // select folder
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.ShowNewFolderButton = false;
            folderDialog.RootFolder = Environment.SpecialFolder.Desktop;

            System.Windows.Forms.DialogResult res = folderDialog.ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                string sPath = folderDialog.SelectedPath;
                string sfile = sPath + @"\names_list.txt";

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(sfile))
                {
                    foreach (NameDataModel name in names)
                    {
                            file.WriteLine(name.Name+",");
                    }
                    // when done, open folder
                    MessageBoxResult res2 = System.Windows.MessageBox.Show("export completed, do you want to open the folder?", "Open Folder", MessageBoxButton.YesNo);
                    if(res2 == MessageBoxResult.Yes)
                    {
                        // open folder
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = sPath,
                            UseShellExecute = true,
                            Verb = "open"
                        });
                    }
                }
            }
        }
        private void ExportToPng(Uri path, Canvas surface)
        {
            if (path == null) return;

            try
            {
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
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)size.Width,(int)size.Height,96d,96d,PixelFormats.Pbgra32);
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

            }catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        private void CreateImageBtn_Click(object sender, RoutedEventArgs e)
        {

            // check if an out folder is selected
            if(exportImagesPath.Length < 1)
            {
                MessageBoxResult res = System.Windows.MessageBox.Show("Please select an output folder first","No Output folder selected");
                if(res == MessageBoxResult.OK)
                {
                    OutputFolderPathInput.Focus();
                }
                return;
            }

            // get name of what is in placeholderTextInput
             var fullFilePath = exportImagesPath + @"\" + placeholderTextInput.Text + ".png";

            if(placeholderTextInput.Text.Length < 1)
            {
                // there is no name in the placeholder control
                System.Windows.MessageBox.Show("Please enter text in the placeholder element");

                // focus on placeholder input elem
                placeholderTextInput.Focus();
                return;
            }


            // check if file name already exists

            if (File.Exists(fullFilePath))
            {
                // show message box with error
                MessageBoxResult res2 =  System.Windows.MessageBox.Show("A file already exists in the folder. Do you want to keep both files? If no, file will be overwtitten","File Exists",MessageBoxButton.YesNo);

                if(res2 == MessageBoxResult.Yes)
                {
                    // rename file
                    fullFilePath = fileNameIfExists(fullFilePath, placeholderTextInput.Text, 1);
                }
            }

            overlayer.RectangleControl.Visibility = Visibility.Hidden;
            overlayer.CanvasControl.UpdateLayout();
            overlayer.updateSelectionArea();

            // call export to png, since it is the single image button we just export what is currently on the canvas
            ExportToPng(new Uri(fullFilePath), overlayer.CanvasControl);

            // when done, open folder
            MessageBoxResult res3 = System.Windows.MessageBox.Show("Image Created, do you want to open the folder?", "Open Folder", MessageBoxButton.YesNo);
            if (res3 == MessageBoxResult.Yes)
            {
                // open folder
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = exportImagesPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }

        private string fileNameIfExists(string pathToFile, string fileName, int currentCount)
        {
            if (File.Exists(pathToFile))
            {
                var fName = fileName;
                string temp = exportImagesPath + @"\" + fName + $"({currentCount})" + ".png";
                int count = currentCount + 1;
                return fileNameIfExists(temp, fileName,count);
            }
            return pathToFile;
        }

        private string dirNameIfExists(string fullDirPath, int currentCount)
        {
            if (Directory.Exists(fullDirPath))
            {
                var dName = DEFAULT_DIR_NAME;
                string temp = exportImagesPath + @"\" + dName + DateTime.Now.ToString("dd-MM-yy") + $"({currentCount})";
                int count = currentCount + 1;
                return dirNameIfExists(temp, count);
            }
            return fullDirPath;
        }

        private void OutputFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            // select folder
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.ShowNewFolderButton = false;
            folderDialog.RootFolder = Environment.SpecialFolder.Desktop;

            System.Windows.Forms.DialogResult res = folderDialog.ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                string sPath = folderDialog.SelectedPath;

                // set global var to location
                exportImagesPath = sPath;

                // update ui inp element
                OutputFolderPathInput.Text = exportImagesPath;
            }
        }

        private void StartBatchCreationBtn_Click(object sender, RoutedEventArgs e)
        {
            // check if an out folder is selected
            if (exportImagesPath.Length < 1)
            {
                MessageBoxResult res1 = System.Windows.MessageBox.Show("Please select an output folder first", "No Output folder selected");
                if (res1 == MessageBoxResult.OK)
                {
                    OutputFolderPathInput.Focus();
                }
                return;
            }

            // create folder
            string dirFullPath = exportImagesPath + @"\" + DEFAULT_DIR_NAME + DateTime.Now.ToString("dd-MM-yy");

            if (Directory.Exists(dirFullPath))
            {
                MessageBoxResult res = System.Windows.MessageBox.Show("A directory already exists with the name. Do you want to add to the existing directory?", "Directory Exists", MessageBoxButton.YesNo);
                
                // recurse by adding count
                if (res == MessageBoxResult.No)
                {
                    // rename folder by adding an increment count
                    dirFullPath = dirNameIfExists(dirFullPath, 1);
                    Directory.CreateDirectory(dirFullPath);
                }
                else
                {
                    // use same directory, do not create any directory
                }
            }
            else
            {
                Directory.CreateDirectory(dirFullPath);
            }

            // save original path
            var bck = exportImagesPath;
            exportImagesPath = dirFullPath;

            ProgressBarWindow progressBarWindow = new ProgressBarWindow(overlayer,dirFullPath,(from name in names select name.Name).ToList());
            progressBarWindow.Owner = GetWindow(this);
            progressBarWindow.ShowDialog();

            // when done, open folder

/*            MessageBoxResult res2 = System.Windows.MessageBox.Show("All images have been created, do you want to view the output folder?", "Batch Creation Completed", MessageBoxButton.YesNo);
            if (res2 == MessageBoxResult.Yes)
            {
                // open folder
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = dirFullPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }*/

            // restore path to original
            exportImagesPath = bck;
        }

        private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //
            Slider elem = (Slider)sender;

            var scale = Math.Round(elem.Value, 2);
            var percent = scale * 100;
            // 
            ZoomSliderLabel.Content = $"{percent}%";

            overlayer.LayoutTransform = new ScaleTransform(scale,scale);
        }

        private void OpenOutputFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (exportImagesPath.Length < 1) return;

            // shell open folder
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = exportImagesPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void FitToViewBtn_Click(object sender, RoutedEventArgs e)
        {
            // calc optimal size
            double optimalZoom = Dimension.calculateOptimalZoom(
                new Dimension(
                    overlayer.ImageControl.ActualWidth,
                    overlayer.ImageControl.ActualHeight),
                new Dimension(
                    ImagePreviewBoxScrollContainer.ActualWidth,
                    ImagePreviewBoxScrollContainer.ActualHeight)
                );

            // add 1 percent padding on either 
            zoomSlider.Value = optimalZoom != 1d ? (optimalZoom - .01d) : optimalZoom;
        }
    }

    public class ActionCommand : ICommand {
        private readonly Action<object> Action;
        private readonly Predicate<object> Predicate;

        public ActionCommand(Action<object> action) : this(action, XmlDataProvider => true) { }
        public ActionCommand(Action<object> action, Predicate<object> predicate) {
            Action = action;
            Predicate = predicate;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        public bool CanExecute(object parameter) {
            return Predicate(parameter);
        }
        public void Execute(object parameter) {
            Action(parameter);
        }
    }

}
