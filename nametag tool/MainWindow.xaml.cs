using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        //
        ObservableCollection<string> names = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();

            OnRemoveBtnClickCommand = new ActionCommand(x => names.Remove(x.ToString()));

            OnPreviewBtnClickCommand = new ActionCommand(x => { overlayer.Text = x.ToString(); placeholderTextInput.Text = x.ToString(); csvNamesListBox.SelectedValue = x; });

            FontSizeCombo.ItemsSource = Enumerable.Range(1, 8).Select(x => x * x);

            sortByCbx.ItemsSource = Enum.GetValues(typeof(OrderBy));
        }

        private void selectCsvBtn_Click(object sender, RoutedEventArgs e)
        {
            // check if names list is not already populated
            if(names.Count > 1)
            {
                MessageBoxResult res = MessageBox.Show("Are you sure you want to select a new CSV file? Note that this will clear the current list", "Clear List",MessageBoxButton.YesNo);

                if (res != MessageBoxResult.Yes) return;
               
               // clear names
                names.Clear();
            }

            // open selection dialog
            OpenFileDialog selectCsvDialog = new OpenFileDialog();
            selectCsvDialog.Multiselect = false;
            selectCsvDialog.Filter = "Comma Separated Value Files|*.csv";

            if (selectCsvDialog.ShowDialog() == true) {
                // parse csv
                
                var fileContent = string.Empty;
                fileContent = System.IO.File.ReadAllText(selectCsvDialog.FileName);

                var namesArr = fileContent.Split(',');

                // set names count label
                namesFoundCountLbl.Content = $"{namesArr.Length} name(s) found in csv file";

                // save csv items in data structure
                for (var i = 0; i < namesArr.Length; i++) {
                    names.Add(namesArr[i]);
                }

                // set datasource for view listbox
                csvNamesListBox.ItemsSource = names;

                names = OrderThoseGroups(names,OrderBy.nameDesc);
            }

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
            }
            // load image to image overlay control
        }

        private void placeholderTextInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox elem = (TextBox)sender;
            overlayer.Text = elem.Text;
            overlayer.CanvasControl.UpdateLayout();
            overlayer.updateSelectionArea();
        }

        public enum OrderBy { 
            nameDesc,
            nameAsc,
            nameLengthAsc,
            nameLengthDesc
        }

        // utility
        public static ObservableCollection<string> OrderThoseGroups(ObservableCollection<string> orderThoseGroups,OrderBy orderBy)
        {
            ObservableCollection<string> temp = orderThoseGroups;

            switch (orderBy)
            {
                case OrderBy.nameAsc:
                    temp = new ObservableCollection<string>(orderThoseGroups.OrderBy(p => p));
                    break;
                case OrderBy.nameDesc:
                    temp = new ObservableCollection<string>(orderThoseGroups.OrderByDescending(p => p));
                    break;
                case OrderBy.nameLengthAsc:
                    temp = new ObservableCollection<string>(orderThoseGroups.OrderBy(p => p.Length));
                    break;
                case OrderBy.nameLengthDesc:
                    temp = new ObservableCollection<string>(orderThoseGroups.OrderByDescending(p => p.Length));
                    break;
            }

            orderThoseGroups.Clear();
            foreach (string j in temp) orderThoseGroups.Add(j);
            return orderThoseGroups;
        }

        private void FontFamilyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox elem = (ComboBox)sender;
            overlayer.TextFontFamily = new FontFamily(elem.SelectedItem.ToString());
            overlayer.CanvasControl.UpdateLayout();
            overlayer.updateSelectionArea();
        }

        private void FontSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox elem = (ComboBox)sender;

            try
            {
                if (elem.SelectedItem != null)
                {
                    double val;

                    if (double.TryParse(elem.SelectedItem.ToString(), out val))
                    {
                        overlayer.TextFontSize = val;
                        overlayer.CanvasControl.UpdateLayout();
                        overlayer.updateSelectionArea();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("value entered is not valid.");
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
            names = OrderThoseGroups(names, sortOrder);
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
        public event EventHandler CanExecuteChanged {
            add {
                CommandManager.RequerySuggested += value;
            }
            remove {
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
