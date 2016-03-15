using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace vJoyMouseFeeder.Entities
{
    public class VJoyDevice
    {
        private vJoy _joystick;
        private vJoy.JoystickState _iReport;

        public bool Connect(uint id)
        {
            // Create one joystick object and a position structure.
            _joystick = new vJoy();
            _iReport = new vJoy.JoystickState();

            // Device ID can only be in the range 1-16
            //if (args.Length>0 && !String.IsNullOrEmpty(args[0]))
            //    id = Convert.ToUInt32(args[0]);
            if (id <= 0 || id > 16)
            {
                Debug.WriteLine("Illegal device ID {0}\nExit!", id);
                return false;
            }

            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!_joystick.vJoyEnabled())
            {
                Debug.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return false;
            }
            else
                Debug.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n", _joystick.GetvJoyManufacturerString(), _joystick.GetvJoyProductString(), _joystick.GetvJoySerialNumberString());

            // Get the state of the requested device
            VjdStat status = _joystick.GetVJDStatus(id);
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
                    return false;
                case VjdStat.VJD_STAT_MISS:
                    Debug.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                    return false;
                default:
                    Debug.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                    return false;
            };

            // Check which axes are supported
            bool AxisX = _joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X);
            bool AxisY = _joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y);
            bool AxisZ = _joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Z);
            bool AxisRX = _joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
            bool AxisRY = _joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RY);
            bool AxisRZ = _joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RZ);
            bool AxisSL0 = _joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_SL0);
            bool AxisSL1 = _joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_SL1);
            bool AxisWHL = _joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_WHL);
            bool AxisPOW = _joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_POV);
            // Get the number of buttons and POV Hat switchessupported by this vJoy device
            int nButtons = _joystick.GetVJDButtonNumber(id);
            int ContPovNumber = _joystick.GetVJDContPovNumber(id);
            int DiscPovNumber = _joystick.GetVJDDiscPovNumber(id);

            // Print results
            Debug.WriteLine("\nvJoy Device {0} capabilities:\n", id);
            Debug.WriteLine("Numner of buttons\t\t{0}\n", nButtons);
            Debug.WriteLine("Numner of Continuous POVs\t{0}\n", ContPovNumber);
            Debug.WriteLine("Numner of Descrete POVs\t\t{0}\n", DiscPovNumber);
            Debug.WriteLine("Axis X\t\t{0}\n", AxisX ? "Yes" : "No");
            Debug.WriteLine("Axis Y\t\t{0}\n", AxisX ? "Yes" : "No");
            Debug.WriteLine("Axis Z\t\t{0}\n", AxisX ? "Yes" : "No");
            Debug.WriteLine("Axis Rx\t\t{0}\n", AxisRX ? "Yes" : "No");
            Debug.WriteLine("Axis Ry\t\t{0}\n", AxisRY ? "Yes" : "No");
            Debug.WriteLine("Axis Rz\t\t{0}\n", AxisRZ ? "Yes" : "No");
            Debug.WriteLine("Axis SL0\t\t{0}\n", AxisSL0 ? "Yes" : "No");
            Debug.WriteLine("Axis SL1\t\t{0}\n", AxisSL1 ? "Yes" : "No");
            Debug.WriteLine("Axis WHL\t\t{0}\n", AxisWHL ? "Yes" : "No");
            Debug.WriteLine("Axis POW\t\t{0}\n", AxisPOW ? "Yes" : "No");

            // Test if DLL matches the driver
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = _joystick.DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                Debug.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            else
                Debug.WriteLine("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n", DrvVer, DllVer);

            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!_joystick.AcquireVJD(id))))
            {
                Debug.WriteLine("Failed to acquire vJoy device number {0}.\n", id);
                return false;
            }
            else
            {
                Debug.WriteLine("Acquired: vJoy device number {0}.\n", id);
                return true;
            }
        }

        public void Test(uint id)
        {
            int X, Y, Z, ZR, XR;
            uint count = 0;
            long maxval = 0;

            X = 20;
            Y = 30;
            Z = 40;
            XR = 60;
            ZR = 80;

            _joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref maxval);

            bool res;
            // Reset this device to default values
            _joystick.ResetVJD(id);

            // Feed the device in endless loop
            while (count < 640)
            {
                // Set position of 4 axes
                res = _joystick.SetAxis(X, id, HID_USAGES.HID_USAGE_X);
                res = _joystick.SetAxis(Y, id, HID_USAGES.HID_USAGE_Y);
                res = _joystick.SetAxis(Z, id, HID_USAGES.HID_USAGE_Z);
                res = _joystick.SetAxis(XR, id, HID_USAGES.HID_USAGE_RX);
                res = _joystick.SetAxis(ZR, id, HID_USAGES.HID_USAGE_RZ);

                // Press/Release Buttons
                res = _joystick.SetBtn(true, id, count / 50);
                res = _joystick.SetBtn(false, id, 1 + count / 50);

                // If Continuous POV hat switches installed - make them go round
                // For high values - put the switches in neutral state
                /*
                if (ContPovNumber > 0)
                {
                    if ((count * 70) < 30000)
                    {
                        res = _joystick.SetContPov(((int)count * 70), id, 1);
                        res = _joystick.SetContPov(((int)count * 70) + 2000, id, 2);
                        res = _joystick.SetContPov(((int)count * 70) + 4000, id, 3);
                        res = _joystick.SetContPov(((int)count * 70) + 6000, id, 4);
                    }
                    else
                    {
                        res = _joystick.SetContPov(-1, id, 1);
                        res = _joystick.SetContPov(-1, id, 2);
                        res = _joystick.SetContPov(-1, id, 3);
                        res = _joystick.SetContPov(-1, id, 4);
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
                        _joystick.SetDiscPov((((int)count / 20) + 0) % 4, id, 1);
                        _joystick.SetDiscPov((((int)count / 20) + 1) % 4, id, 2);
                        _joystick.SetDiscPov((((int)count / 20) + 2) % 4, id, 3);
                        _joystick.SetDiscPov((((int)count / 20) + 3) % 4, id, 4);
                    }
                    else
                    {
                        _joystick.SetDiscPov(-1, id, 1);
                        _joystick.SetDiscPov(-1, id, 2);
                        _joystick.SetDiscPov(-1, id, 3);
                        _joystick.SetDiscPov(-1, id, 4);
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
    }
}
