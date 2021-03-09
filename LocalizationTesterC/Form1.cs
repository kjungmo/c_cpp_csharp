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
using System.Globalization;

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
            Console.Write("IetfLanguageTag : ");
            Console.WriteLine(Thread.CurrentThread.CurrentUICulture.IetfLanguageTag);
            Console.Write("TextInfo : ");
            Console.WriteLine(CultureInfo.CurrentCulture.TextInfo);
            Console.Write("CultureTypes : ");
            Console.WriteLine(CultureInfo.CurrentCulture.CultureTypes);
            Console.Write("DisplayName : ");
            Console.WriteLine(CultureInfo.CurrentCulture.DisplayName);
            Console.Write("EnglishName : ");
            Console.WriteLine(CultureInfo.CurrentCulture.EnglishName);
            Console.Write("TwoLetterISOLanguageName : ");
            Console.WriteLine(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            Console.Write("LCID : ");
            Console.WriteLine(CultureInfo.CurrentCulture.LCID);
            Console.Write("Name : ");
            Console.WriteLine(CultureInfo.CurrentCulture.Name);
            Console.Write("NativeName : ");
            Console.WriteLine(CultureInfo.CurrentCulture.NativeName);
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0: Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ko-KR");break;
                case 1: Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");break;
            }
            this.Controls.Clear();
            InitializeComponent();
            Form1_Load(sender, e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("한국어");
            comboBox1.Items.Add("English");
        }
    }
}
