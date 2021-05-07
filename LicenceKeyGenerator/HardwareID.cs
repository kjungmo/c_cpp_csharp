using System;
using System.Collections.Generic;
using System.Management;
using System.Text;

namespace LicenceKeyGenerator
{
    class HardwareID
    {
        public static string MbSerial { get; set; }

        public static List<string> MACAddress = new List<string>();
        public static List<string> Diskdrives = new List<string>();


        public static string KeyGenerator(string macAddress)
        {
            byte[] bytes;
            byte[] hasedBytes;
            StringBuilder sb = new StringBuilder();

            ManagementObjectSearcher mBoard = new ManagementObjectSearcher("SELECT * FROM Win32_baseboard");
            ManagementObjectCollection mBoard_Collection = mBoard.Get();

            foreach (ManagementObject obj in mBoard_Collection)
            {
                sb.Append(obj["SerialNumber"].ToString());
                MbSerial = obj["SerialNumber"].ToString();
                break;
            }
            if (string.IsNullOrEmpty(macAddress))
            {
                return null;
            }
            sb.Append(macAddress);
            ManagementObjectSearcher diskDrive = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            ManagementObjectCollection diskDrive_Collection = diskDrive.Get();

            foreach (ManagementObject obj in diskDrive_Collection)
            {
                sb.Append(obj["SerialNumber"].ToString());
                Diskdrives.Add(obj["SerialNumber"].ToString() + "\r");
            }

            bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString().Substring(0, sb.ToString().Length - 1));
            hasedBytes = System.Security.Cryptography.SHA256.Create().ComputeHash(bytes);
            return Convert.ToBase64String(hasedBytes);
        }

        
    }
}
