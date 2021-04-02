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
using System.Windows;

namespace LocalizationTesterD
{
    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
    public partial class Form1 : Form
    {

        Tools.LogManager logger;

        public Form1(Tools.LogManager logger)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ko-KR");
            InitializeComponent();
            SetChangedLanguageComponents();

            label1.Text = "라벨 원";
            label1.Text = Languages.Resource.ResourceManager.GetString("Label1");

            this.logger = logger;
            //throw new AccessViolationException();
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

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            logger.WriteLine($"from closing {e.CloseReason}");
            if (MessageBox.Show("Exit?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void Form_FormClosed(Object sender, FormClosedEventArgs e)
        {
            logger.WriteLine($"from closed {e.CloseReason}");
            StringBuilder messageBoxCS = new StringBuilder();
            messageBoxCS.AppendFormat("{0} = {1}", "CloseReason", e.CloseReason);
            messageBoxCS.AppendLine();
            MessageBox.Show(messageBoxCS.ToString(), "FormClosed Event");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"Button Thread : {Thread.CurrentThread.ManagedThreadId}");
            //throw new Exception("same thread");
            Console.WriteLine($"UI: {Thread.CurrentThread.ManagedThreadId}");
            new Thread(new ThreadStart((() =>
            {
                Console.WriteLine($"NEW THREAD: {Thread.CurrentThread.ManagedThreadId}");
                throw new Exception("New Thread in Form1( )");
            }))).Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FineLocalizerVehicleEngineAPI.LogCallback = logger.WriteLine;
            float a = 0.0f, b = 0.0f, c = 0.0f, d = 0.0f, f = 0.0f;
            FineLocalizerVehicleEngineAPI.EstimateVehicleShift(out IntPtr sigSeg, 0, "", "", "", 0, 9, 9, 9, out IntPtr point, out IntPtr ou, out IntPtr sp, ref a, ref b, ref c, ref d, ref f);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FineLocalizerVehicleEngineAPI.LogCallback = logger.WriteLine;
            FineLocalizerVehicleEngineAPI.EstimateGapScanPose(out IntPtr sof, 0, "", 98, 11);
        }
    }
}
