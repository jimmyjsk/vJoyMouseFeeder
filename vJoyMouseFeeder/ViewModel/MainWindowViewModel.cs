using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vJoyMouseFeeder.Entities;

namespace vJoyMouseFeeder.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            Tracker = MouseTracker.GetTracker();
            Tracker.Start();
            JoystickDevice = new VJoyDevice();
        }

        public MouseTracker Tracker { get; set; }
        public VJoyDevice JoystickDevice { get; set; }

        public void Dispose()
        {
            if (Tracker != null && Tracker.Running)
                Tracker.Stop();
        }
    }
}
