using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Telhai.CS.CSharpCourse.Events
{
    /// <summary>
    /// Interaction logic for SubWindow.xaml
    /// </summary>
    public partial class SubWindow : Window
    {
        public SubWindow()
        {
            InitializeComponent();

        }



        public void MainWindow_ColorChanged(object? sender, Color e)
        {
            SubWindow subWindow = new SubWindow();
            this.Background = new SolidColorBrush(e);
        }



    }
}
