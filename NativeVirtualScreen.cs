using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Darkshot
{
    internal class NativeVirtualScreen
    {
        #region Class section
        public static Rectangle Bounds { get { return SystemInformation.VirtualScreen; } }

        static NativeVirtualScreen()
        {
            //// Initialize the virtual screen to dummy values
            //int left = int.MaxValue;
            //int top = int.MaxValue;
            //int right = int.MinValue;
            //int bottom = int.MinValue;

            //// Enumerate system display devices
            //int deviceIndex = 0;
            //while (true)
            //{
            //    DisplayDevice deviceData = new DisplayDevice { cb = Marshal.SizeOf(typeof(DisplayDevice)) };
            //    if (EnumDisplayDevices(null, deviceIndex, ref deviceData, 0) == 0)
            //        break;

            //    // Get the position and size of this particular display device
            //    DEVMODE devMode = new DEVMODE();
            //    if (EnumDisplaySettings(deviceData.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
            //    {
            //        // Update the virtual screen dimensions
            //        left = Math.Min(left, devMode.dmPositionX);
            //        top = Math.Min(top, devMode.dmPositionY);
            //        right = Math.Max(right, devMode.dmPositionX + devMode.dmPelsWidth);
            //        bottom = Math.Max(bottom, devMode.dmPositionY + devMode.dmPelsHeight);
            //    }
            //    deviceIndex++;
            //}

            //Bounds = new Rectangle(left, top, right - left, bottom - top);
        }
        #endregion

        #region Native section
        [Flags()]
        enum DisplayDeviceStateFlags : int
        {
            /// <summary>The device is part of the desktop.</summary>
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            /// <summary>This is the primary display.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VGACompatible = 0x16,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct DisplayDevice
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll")]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        const int ENUM_CURRENT_SETTINGS = -1;
        const int ENUM_REGISTRY_SETTINGS = -2;

        [DllImport("User32.dll")]
        static extern int EnumDisplayDevices(string lpDevice, int iDevNum, ref DisplayDevice lpDisplayDevice, int dwFlags);
    }
    #endregion
}
