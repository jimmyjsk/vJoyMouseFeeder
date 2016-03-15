using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vJoyMouseFeeder.Entities
{
    public class MouseTracker : INotifyPropertyChanged
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(ref Point lpPoint);

        public event PropertyChangedEventHandler PropertyChanged;

        private static MouseTracker _tracker;
        private Thread _thread;
        private bool _cont = true;
        private bool _running = false;
        private int _x;
        private int _y;
        private MouseTracker()
        {

            _thread = new Thread(new ThreadStart(ThreadStartMethod));
        }

        public int X
        {
            get
            {
                return _x;
            }
            set
            {
                if (_x == value)
                    return;
                _x = value;
                if(PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("X"));
            }
        }
        public int Y
        {
            get
            {
                return _y;
            }
            set
            {
                if (_y == value)
                    return;
                _y = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Y"));
            }
        }

        public bool Running
        {
            get
            {
                return _running;
            }
        }

        public void Start()
        {
            _cont = true;
            _thread.Start();
        }

        public void Stop()
        {
            _cont = false;
        }

        public void ThreadStartMethod()
        {
            _running = true;
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Running"));
            Debug.WriteLine("CaptureThreadStarted");
            while (_cont)
            {
                //Logic
                Point p = new Point();
                GetCursorPos(ref p);

                this.X = p.X;
                this.Y = p.Y;
                Thread.Sleep(5);
            }
            Debug.WriteLine("CaptureThreadStopped");
            _running = false;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Running"));
        }

        public static MouseTracker GetTracker()
        {
            if (_tracker == null)
                _tracker = new MouseTracker();
            return _tracker;
        }
    }
}
