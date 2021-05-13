using System;
using System.Collections.Generic;
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
            
            tbLicense.Text = HardwareID.KeyGenerator(HardwareID.MACAddress.ToString());

            tbMb.Text = HardwareID.MbSerial;
			foreach (var item in HardwareID.MACAddress)
			{
                tbMac.Text += item;
			}
            foreach (var item in HardwareID.Diskdrives)
            {
                tbDiskdrive.Text += item;
            }
            RegeditHelper.Register("License Key", tbLicense.Text);
			Console.WriteLine(HardwareID.ssbb);
		}
    }
}
