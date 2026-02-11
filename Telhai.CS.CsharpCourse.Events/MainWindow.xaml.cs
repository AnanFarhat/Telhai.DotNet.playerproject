using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Telhai.CS.CSharpCourse.Events
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //01) define the delegate  from the event
        public event EventHandler<Color> ColorChanged;

        public MainWindow()
        {
            InitializeComponent();
            GetColore();
            this.Title += Environment.ProcessId.ToString();
            this.Title += Environment.MachineName.ToString();
            this.Title += Environment.OSVersion.ToString();
            this.Title += Guid.NewGuid().ToString();
        }

        public void /*List<string>*/ GetColore()
        {
            //1)Use reflection to get all colors from class :Colors
            PropertyInfo[] colorsProps = typeof(Colors)
                .GetProperties(System.Reflection.BindingFlags.Public
                | BindingFlags.Static);

            object obj = colorsProps[0].GetValue(null);
            if (obj is Color color)
            {
                //Use the color variable here
            }

            var colors =
                       colorsProps.Select(p => new
                       {
                           Name = p.Name,
                           Brush = new SolidColorBrush((Color)p.GetValue(null))
                       })
                       .OrderBy(c => c.Name)
                       .ToList();

            this.lstColors.ItemsSource = colors;
        }
        /// <summary>
        /// Handles the event that occurs when the selection changes in the colors list.
        /// </summary>
        /// <param name="sender">The source of the event, typically the colors list control.</param>
        /// <param name="e">data object related to the event</param>
        private void lstColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox lb)
            {
                if (lb.SelectedItems is not null)
                {
                    dynamic selectedColor = lb.SelectedItem;
                    if (selectedColor.Brush is SolidColorBrush brushColor)
                    {
                        Color colorSelected = brushColor.Color;
                        //02-- Raise the custom event
                        if (ColorChanged != null)
                        {
                            ColorChanged.Invoke(this, colorSelected);
                        }
                    }


                    //Get the selected color

                    // Color color = ((SolidColorBrush)selectedColor.Brush)
                    //     .Color;
                    //RaiseEvent the event
                }
            }
        }

        public void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SubWindow subWindow = new SubWindow();
            //this.ColorChanged += subWindow_ColorChanged;
            this.ColorChanged += subWindow.MainWindow_ColorChanged;
            subWindow.Show();
        }


    }
}