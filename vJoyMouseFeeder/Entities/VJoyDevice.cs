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
        static public vJoy joystick;
        static public vJoy.JoystickState iReport;
        static public uint id = 1;

        public void Test()
        {
            //PositionLabel.Content = "X: " + p.X + ", Y:" + p.Y;

            // Create one joystick object and a position structure.
            joystick = new vJoy();
            iReport = new vJoy.JoystickState();

            // Device ID can only be in the range 1-16
            //if (args.Length>0 && !String.IsNullOrEmpty(args[0]))
            //    id = Convert.ToUInt32(args[0]);
            if (id <= 0 || id > 16)
            {
                Debug.WriteLine("Illegal device ID {0}\nExit!",id); 
                return;
            }

            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!joystick.vJoyEnabled())
            {
                Debug.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }
            else
                Debug.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n", joystick.GetvJoyManufacturerString(), joystick.GetvJoyProductString(), joystick.GetvJoySerialNumberString());

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

            // Check which axes are supported
            bool AxisX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X);
            bool AxisY = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y);
            bool AxisZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Z);
            bool AxisRX = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
            bool AxisRY = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RY);
            bool AxisRZ = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RZ);
            bool AxisSL0 = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_SL0);
            bool AxisSL1 = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_SL1);
            bool AxisWHL = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_WHL);
            bool AxisPOW = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_POV);
            // Get the number of buttons and POV Hat switchessupported by this vJoy device
            int nButtons = joystick.GetVJDButtonNumber(id);
            int ContPovNumber = joystick.GetVJDContPovNumber(id);
            int DiscPovNumber = joystick.GetVJDDiscPovNumber(id);

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
            bool match = joystick.DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                Debug.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            else
                Debug.WriteLine("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n", DrvVer, DllVer);


            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))))
            {
                Debug.WriteLine("Failed to acquire vJoy device number {0}.\n", id);
                return;
            }
            else
                Debug.WriteLine("Acquired: vJoy device number {0}.\n", id);

            int X, Y, Z, ZR, XR;
            uint count = 0;
            long maxval = 0;

            X = 20;
            Y = 30;
            Z = 40;
            XR = 60;
            ZR = 80;

            joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref maxval);

            bool res;
            // Reset this device to default values
            joystick.ResetVJD(id);

            // Feed the device in endless loop
            while (true)
            {
                // Set position of 4 axes
                res = joystick.SetAxis(X, id, HID_USAGES.HID_USAGE_X);
                res = joystick.SetAxis(Y, id, HID_USAGES.HID_USAGE_Y);
                res = joystick.SetAxis(Z, id, HID_USAGES.HID_USAGE_Z);
                res = joystick.SetAxis(XR, id, HID_USAGES.HID_USAGE_RX);
                res = joystick.SetAxis(ZR, id, HID_USAGES.HID_USAGE_RZ);

                // Press/Release Buttons
                res = joystick.SetBtn(true, id, count / 50);
                res = joystick.SetBtn(false, id, 1 + count / 50);

                // If Continuous POV hat switches installed - make them go round
                // For high values - put the switches in neutral state
                if (ContPovNumber > 0)
                {
                    if ((count * 70) < 30000)
                    {
                        res = joystick.SetContPov(((int)count * 70), id, 1);
                        res = joystick.SetContPov(((int)count * 70) + 2000, id, 2);
                        res = joystick.SetContPov(((int)count * 70) + 4000, id, 3);
                        res = joystick.SetContPov(((int)count * 70) + 6000, id, 4);
                    }
                    else
                    {
                        res = joystick.SetContPov(-1, id, 1);
                        res = joystick.SetContPov(-1, id, 2);
                        res = joystick.SetContPov(-1, id, 3);
                        res = joystick.SetContPov(-1, id, 4);
                    };
                };

                // If Discrete POV hat switches installed - make them go round
                // From time to time - put the switches in neutral state
                if (DiscPovNumber > 0)
                {
                    if (count < 550)
                    {
                        joystick.SetDiscPov((((int)count / 20) + 0) % 4, id, 1);
                        joystick.SetDiscPov((((int)count / 20) + 1) % 4, id, 2);
                        joystick.SetDiscPov((((int)count / 20) + 2) % 4, id, 3);
                        joystick.SetDiscPov((((int)count / 20) + 3) % 4, id, 4);
                    }
                    else
                    {
                        joystick.SetDiscPov(-1, id, 1);
                        joystick.SetDiscPov(-1, id, 2);
                        joystick.SetDiscPov(-1, id, 3);
                        joystick.SetDiscPov(-1, id, 4);
                    };
                };

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
