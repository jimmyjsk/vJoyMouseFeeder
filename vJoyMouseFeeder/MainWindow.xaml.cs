using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Point = System.Drawing.Point;
using vJoyInterfaceWrap;
using System.Diagnostics;
using vJoyMouseFeeder.Entities;
using vJoyMouseFeeder.ViewModel;

namespace vJoyMouseFeeder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _context;

        public MainWindow()
        {
            InitializeComponent();
            _context = new MainWindowViewModel();
            this.DataContext = _context;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (DataContext != null && DataContext is MainWindowViewModel)
                ((MainWindowViewModel)DataContext).Dispose();
        }

        //using simple event handlers for testing.
        // TODO refactor to commands
        private void InitializeButton_Click(object sender, RoutedEventArgs e)
        {
            _context.JoystickDevice.Connect(1);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _context.JoystickDevice.Test(1);
        }
    }
}
