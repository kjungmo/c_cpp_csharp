using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LicenceKeyGenerator
{
    public partial class LicenseKeyGenerator : Form
    {
        public LicenseKeyGenerator()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HardwareID.MbSerial = "";
            HardwareID.MACAddress = new List<string>();
            HardwareID.Diskdrives = new List<string>();
            foreach (Control ctrlTextBox in this.Controls)
            {
                if (typeof(TextBox) == ctrlTextBox.GetType())
                {
                    (ctrlTextBox as TextBox).Text = "";
                }
            }

            
            tbLicense.Text = HardwareID.KeyGenerator(getMACAddress());

            tbMb.Text = HardwareID.MbSerial;
            tbMac.Text = getMACAddress();

            foreach (var item in HardwareID.Diskdrives)
            {
                tbDiskdrive.Text += item;
            }
        }

        public NetworkInterface GetCurrentOnlineNetworkInterface()
        {
            for (var i = 0; i < NetworkInterface.GetAllNetworkInterfaces().Length; i++)
            {
                var ni = NetworkInterface.GetAllNetworkInterfaces()[i];

                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                    !ni.Name.ToLower().Contains("loopback"))
                    return ni;
            }
            return null;
        }

        public string getMACAddress()
        {
            var targetInterface = GetCurrentOnlineNetworkInterface();
            var guid = targetInterface.Id;
            string mac = "";
            using (var reg = Registry.LocalMachine.OpenSubKey("SYSTEM").
                OpenSubKey("CurrentControlSet").
                OpenSubKey("Control").
                OpenSubKey("Class").
                OpenSubKey("{4d36e972-e325-11ce-bfc1-08002be10318}"))
            {
                var subKeyNames = reg.GetSubKeyNames();

                foreach (var subKeyName in subKeyNames)
                {
                    if (!Regex.IsMatch(subKeyName, @"\d{4}"))
                    {
                        continue;
                    }

                    using (var subKey = reg.OpenSubKey(subKeyName, true))
                    {
                        if (subKey.GetValue("NetworkAddress") == null)
                        {
                            return targetInterface.GetPhysicalAddress().ToString();
                        }
                        return null;
                    }
                }
                return null;
            }
        }
    }
}
