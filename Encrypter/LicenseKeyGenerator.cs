using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace Encrypter
{
    class LicenseKeyGenerator
    {
        public static string MbSerial { get; set; }

        public static List<string> MACAddress = new List<string>();
        public static List<string> Diskdrives = new List<string>();

        public static string KeyGenerator()
        {
            byte[] bytes;
            byte[] hasedBytes;
            StringBuilder sb = new StringBuilder();

            ManagementObjectSearcher mBoard = new ManagementObjectSearcher("SELECT * FROM Win32_baseboard");
            ManagementObjectCollection mBoard_Collection = mBoard.Get();

            foreach (ManagementObject obj in mBoard_Collection)
            {
                sb.Append(obj["SerialNumber"].ToString() + "-");
                MbSerial = obj["SerialNumber"].ToString();
                break;
            }

            ManagementObjectSearcher net = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter");
            ManagementObjectCollection net_Collection = net.Get();

            foreach (ManagementObject obj in net_Collection)
            {
                if (obj["DeviceID"].ToString() != "3")
                {
                    continue;
                }
                sb.Append(obj["MACAddress"].ToString() + "-");
                MACAddress.Add(obj["MACAddress"].ToString());
            }

            ManagementObjectSearcher diskDrive = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            ManagementObjectCollection diskDrive_Collection = diskDrive.Get();

            foreach (ManagementObject obj in diskDrive_Collection)
            {
                sb.Append(obj["SerialNumber"].ToString() + "/");
                Diskdrives.Add(obj["SerialNumber"].ToString() + "\r");
            }

            bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString().Substring(0, sb.ToString().Length - 1));
            hasedBytes = System.Security.Cryptography.SHA256.Create().ComputeHash(bytes);
            return Convert.ToBase64String(hasedBytes);
        }
    }
}
