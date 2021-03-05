using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LocalizationTesterC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ko-KR");
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (Thread.CurrentThread.CurrentUICulture.IetfLanguageTag)
            {
                case "ko-KR": Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en"); break;
                case "en": Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ko-KR"); break;
                //default:
                //    break;
            }
            this.Controls.Clear();
            Console.WriteLine(Thread.CurrentThread.CurrentUICulture.IetfLanguageTag);
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
