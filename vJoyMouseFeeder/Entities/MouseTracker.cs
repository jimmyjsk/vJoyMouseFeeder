using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private Thread thread;
        private bool cont = true;
        private int _x;
        private int _y;
        private MouseTracker()
        {

            thread = new Thread(new ThreadStart(ThreadStartMethod));
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

        public void Start()
        {
            cont = true;
            thread.Start();
        }

        public void Stop()
        {
            cont = false;
        }

        public void ThreadStartMethod()
        {
            while (cont)
            {
                //Logic
                Point p = new Point();
                GetCursorPos(ref p);

                this.X = p.X;
                this.Y = p.Y;
                Thread.Sleep(5);
            }
        }

        public static MouseTracker GetTracker()
        {
            if (_tracker == null)
                _tracker = new MouseTracker();
            return _tracker;
        }
    }
}
