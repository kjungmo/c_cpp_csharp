using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;

namespace LocalizationTesterD
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ko-KR");
            InitializeComponent();
            SetChangedLanguageComponents();

            label1.Text = "라벨 원";
            label1.Text = Languages.Resource.ResourceManager.GetString("Label1");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0: Thread.CurrentThread.CurrentUICulture = new CultureInfo("ko-KR"); break;
                case 1: Thread.CurrentThread.CurrentUICulture = new CultureInfo("en"); break;
            }
            SetChangedLanguageComponents();
        }

        private void SetChangedLanguageComponents()
        {
            this.Text = Languages.Resource.this_Text;
            this.button1.Text = Languages.Resource.button1_Text;
            this.groupBox1.Text = Languages.Resource.groupBox1_Text;
            this.label1.Text = Languages.Resource.label1_Text;
            this.label2.Text = Languages.Resource.label2_Text;
            this.richTextBox1.Text = Languages.Resource.richTextBox1_Text;

        }
    }
}
