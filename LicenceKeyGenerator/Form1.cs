using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
            foreach (Control ctrlTextBox in this.Controls)
            {
                if (typeof(TextBox) == ctrlTextBox.GetType())
                {
                    (ctrlTextBox as TextBox).Text = "";
                }
            }
            tbLicense.Text = HardwareID.GET_HARDWAREID;

            tbMb.Text = HardwareID.MbSerial;
            foreach (var item in HardwareID.MACAddress)
            {
                tbMac.Text += item;
            }
            foreach (var item in HardwareID.Diskdrives)
            {
                tbDiskdrive.Text += item;
            }

        }
    }
}
