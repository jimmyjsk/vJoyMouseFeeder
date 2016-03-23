using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace vJoyMouseFeeder.Entities
{
    public class VJoyDevice
    {
        private uint _id;
        private vJoy _joystick;
        private vJoy.JoystickState _iReport;
        private Thread _thread;
        private bool _cont = true;
        private bool _feeding = false;
        private MouseTracker _tracker;

        public bool Connected { get; set; }
        public bool Feeding { get { return _feeding; } }


        public VJoyDevice()
        {
            // Create one joystick object and a position structure.
            _joystick = new vJoy();
            _iReport = new vJoy.JoystickState();
            Connected = false;
            _thread = new Thread(new ThreadStart(ThreadStartMethod));
        }

        public bool Connect(uint id)
        {
            // Device ID can only be in the range 1-16
            //if (args.Length>0 && !String.IsNullOrEmpty(args[0]))
            //    id = Convert.ToUInt32(args[0]);
            if (id <= 0 || id > 16)
            {
                Debug.WriteLine("Illegal device ID " + id);
                return false;
            }

            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!_joystick.vJoyEnabled())
            {
                Debug.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.");
                return false;
            }
            else
            {
                Debug.WriteLine("Vendor: " + _joystick.GetvJoyManufacturerString());
                Debug.WriteLine("Product :" + _joystick.GetvJoyProductString());
                Debug.WriteLine("Version Number:" + _joystick.GetvJoySerialNumberString());
            }

            // Get the state of the requested device
            VjdStat status = _joystick.GetVJDStatus(id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    Debug.WriteLine("vJoy Device " + id + " is already owned by this feeder");
                    break;
                case VjdStat.VJD_STAT_FREE:
                    Debug.WriteLine("vJoy Device " + id + " is free");
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    Debug.WriteLine("vJoy Device " + id + " is already owned by another feeder. Cannot continue");
                    return false;
                case VjdStat.VJD_STAT_MISS:
                    Debug.WriteLine("vJoy Device " + id + " is not installed or disabled. Cannot continue");
                    return false;
                default:
                    Debug.WriteLine("vJoy Device " + id + " general error. Cannot continue");
                    return false;
            };

            // Test if DLL matches the driver
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = _joystick.DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                Debug.WriteLine("Version of Driver Matches DLL Version " + DllVer);
            else
                Debug.WriteLine("Version of Driver (" + DrvVer +") does NOT match DLL Version (" + DllVer + ")");

            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!_joystick.AcquireVJD(id))))
            {
                Debug.WriteLine("Failed to acquire vJoy device number " + id + ".");
                return false;
            }
            else
            {
                Debug.WriteLine("Acquired: vJoy device number " + id + ".");
                _id = id;
                Connected = true;
                return true;
            }
        }

        public bool GetCapabilities()
        {
            if (!Connected)
                return false;

            // Get the number of buttons and POV Hat switchessupported by this vJoy device
            int nButtons = _joystick.GetVJDButtonNumber(_id);
            int contPovNumber = _joystick.GetVJDContPovNumber(_id);
            int discPovNumber = _joystick.GetVJDDiscPovNumber(_id);

            // Print results
            Debug.WriteLine("vJoy Device " + _id + " capabilities:");
            Debug.WriteLine("Numner of buttons\t\t" + nButtons);
            Debug.WriteLine("Numner of Continuous POVs\t" + contPovNumber);
            Debug.WriteLine("Numner of Descrete POVs\t\t" + discPovNumber);

            foreach (HID_USAGES axis in Enum.GetValues(typeof(HID_USAGES)))
            {
                AxisIsSupported(axis);
                GetAxisMax(axis);
            }
            return true;
        }

        public void StartFeeding(MouseTracker tracker)
        {
            _cont = true;
            _tracker = tracker;
            _thread.Start();
        }

        public void StopFeeding()
        {
            _cont = false;
        }

        private void ThreadStartMethod()
        {
            int prevMouseX = _tracker.X;
            int prevMouseY = _tracker.Y;
            int currentMouseX = prevMouseX;
            int currentMouseY = prevMouseY;
            int joystickX = 200;
            int joystickY = 200;
            int mouseDeltaX = 0;
            int mouseDeltaY = 0;
            double coeficientX = GetAxisMax(HID_USAGES.HID_USAGE_X) / 400;
            double coeficientY = GetAxisMax(HID_USAGES.HID_USAGE_Y) / 400;
            _feeding = true;
            Debug.WriteLine("FeedingThreadStarted");
            while (_cont)
            {
                currentMouseX = _tracker.X;
                currentMouseY = _tracker.Y;
                mouseDeltaX = currentMouseX - prevMouseX;
                mouseDeltaY = currentMouseY - prevMouseY;
                prevMouseX = currentMouseX;
                prevMouseY = currentMouseY;
                joystickX += mouseDeltaX;
                joystickY += mouseDeltaY;
                if (joystickX < 0) 
                    joystickX = 0;
                if (joystickX > 400)
                    joystickX = 400;
                if (joystickY < 0)
                    joystickY = 0;
                if (joystickY > 400)
                    joystickY = 400;
                SetAxisValue(HID_USAGES.HID_USAGE_X, (int)(joystickX * coeficientX));
                SetAxisValue(HID_USAGES.HID_USAGE_Y, (int)(joystickY * coeficientY));
                Thread.Sleep(10);
            }
            Debug.WriteLine("FeedingThreadStopped");
            _feeding = false;
            _tracker = null;
        }

        public void Test()
        {
            int X, Y, Z, ZR, XR;
            uint count = 0;
            long maxval = GetAxisMax(HID_USAGES.HID_USAGE_X);

            X = 20;
            Y = 30;
            Z = 40;
            XR = 60;
            ZR = 80;

            bool res;
            // Reset this device to default values
            _joystick.ResetVJD(_id);

            // Feed the device in endless loop
            while (count < 640)
            {
                // Set position of 4 axes
                res = _joystick.SetAxis(X, _id, HID_USAGES.HID_USAGE_X);
                res = _joystick.SetAxis(Y, _id, HID_USAGES.HID_USAGE_Y);
                res = _joystick.SetAxis(Z, _id, HID_USAGES.HID_USAGE_Z);
                res = _joystick.SetAxis(XR, _id, HID_USAGES.HID_USAGE_RX);
                res = _joystick.SetAxis(ZR, _id, HID_USAGES.HID_USAGE_RZ);

                // Press/Release Buttons
                res = _joystick.SetBtn(true, _id, count / 50);
                res = _joystick.SetBtn(false, _id, 1 + count / 50);

                // If Continuous POV hat switches installed - make them go round
                // For high values - put the switches in neutral state
                /*
                if (ContPovNumber > 0)
                {
                    if ((count * 70) < 30000)
                    {
                        res = _joystick.SetContPov(((int)count * 70), _id, 1);
                        res = _joystick.SetContPov(((int)count * 70) + 2000, _id, 2);
                        res = _joystick.SetContPov(((int)count * 70) + 4000, _id, 3);
                        res = _joystick.SetContPov(((int)count * 70) + 6000, _id, 4);
                    }
                    else
                    {
                        res = _joystick.SetContPov(-1, _id, 1);
                        res = _joystick.SetContPov(-1, _id, 2);
                        res = _joystick.SetContPov(-1, _id, 3);
                        res = _joystick.SetContPov(-1, _id, 4);
                    };
                };
                */
                /*
                // If Discrete POV hat switches installed - make them go round
                // From time to time - put the switches in neutral state
                if (DiscPovNumber > 0)
                {
                    if (count < 550)
                    {
                        _joystick.SetDiscPov((((int)count / 20) + 0) % 4, _id, 1);
                        _joystick.SetDiscPov((((int)count / 20) + 1) % 4, _id, 2);
                        _joystick.SetDiscPov((((int)count / 20) + 2) % 4, _id, 3);
                        _joystick.SetDiscPov((((int)count / 20) + 3) % 4, _id, 4);
                    }
                    else
                    {
                        _joystick.SetDiscPov(-1, _id, 1);
                        _joystick.SetDiscPov(-1, _id, 2);
                        _joystick.SetDiscPov(-1, _id, 3);
                        _joystick.SetDiscPov(-1, _id, 4);
                    };
                };
                */
                System.Threading.Thread.Sleep(20);
                //Debug.WriteLine("Mouse position - x: " + tracker.X + ", y: " + tracker.Y);
                X += 150; if (X > maxval) X = 0;
                Y += 250; if (Y > maxval) Y = 0;
                Z += 350; if (Z > maxval) Z = 0;
                XR += 220; if (XR > maxval) XR = 0;
                ZR += 200; if (ZR > maxval) ZR = 0;
                count++;

                if (count > 640)
                    count = 0;

            } // While (Robust)
        }

        private long GetAxisMax(HID_USAGES axis)
        {
            long maxval = 0;
            _joystick.GetVJDAxisMax(_id, axis, ref maxval);
            Debug.WriteLine("Axis: " + axis.ToString() + ", Maxvalue: " + maxval);
            return maxval;
        }

        private bool AxisIsSupported(HID_USAGES axis)
        {
            bool axisSupported = _joystick.GetVJDAxisExist(_id, axis);
            Debug.WriteLine("Axis: " + axis.ToString() + ", Supported: " + axisSupported);
            return axisSupported;
        }

        private bool SetAxisValue(HID_USAGES axis, int value)
        {
            return _joystick.SetAxis(value, _id, axis);
        }
    }
}
