using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LicenseGenerator
{
	public partial class LicenseGenerator : Form
	{
		public LicenseGenerator()
		{
			InitializeComponent();
		}

		private void btnGenerate_Click(object sender, EventArgs e)
		{
			var li = Generator.GenerateLicenseKey(false);
			string key = Marshal.PtrToStringAnsi(li);
			tbLicense.Text = key;
		}
	}
}
