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

namespace vJoyMouseFeeder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(ref Point lpPoint);

        static public vJoy joystick;
        static public vJoy.JoystickState iReport;
        static public uint id = 1;


        public MainWindow()
        {
            InitializeComponent();

            new Thread(() =>
            {
                while (true)
                {
                    //Logic
                    Point p = new Point();
                    GetCursorPos(ref p);

                    //Update UI
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        PositionLabel.Content = "X: " + p.X + ", Y:" + p.Y;
                    }));

                    Thread.Sleep(10);
                }
            }).Start();

            // Create one joystick object and a position structure.
            joystick = new vJoy();
            iReport = new vJoy.JoystickState();

            // Device ID can only be in the range 1-16
            //if (args.Length>0 && !String.IsNullOrEmpty(args[0]))
            //    id = Convert.ToUInt32(args[0]);
            if (id <= 0 || id > 16)
            {
                Console.WriteLine("Illegal device ID {0}\nExit!",id); 
                return;
            }

            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!joystick.vJoyEnabled())
            {
                Debug.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }
            else
                Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n", joystick.GetvJoyManufacturerString(), joystick.GetvJoyProductString(), joystick.GetvJoySerialNumberString());

            // Get the state of the requested device
            VjdStat status = joystick.GetVJDStatus(id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    Debug.WriteLine("vJoy Device {0} is already owned by this feeder\n", id);
                    break;
                case VjdStat.VJD_STAT_FREE:
                    Debug.WriteLine("vJoy Device {0} is free\n", id);
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    Debug.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                    return;
                case VjdStat.VJD_STAT_MISS:
                    Debug.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                    return;
                default:
                    Debug.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                    return;
            };
        }
    }
}
