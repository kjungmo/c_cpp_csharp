using System;
using System.Drawing;
using System.Windows.Forms;
using CommonUtils;


namespace FineLocalizer
{
    public partial class FineLocalizerDashboard_ : Form
    {
        private NgListViewManager _ngListView;

        public FineLocalizerDashboard_(string carNumber, string carName, string carSeqNum)
        {
            InitializeComponent();
            this.ApplyFont();
            _ngListView = new NgListViewManager(this.ltvNgList,18,25);
            ltvNgList.Font = new Font("Consolas", 15f);
            LoadCarInfo(carNumber, carName);
            LoadCarSeqNum(carSeqNum);
            this.Translate(Lang.DashB.ResourceManager);
        }

        protected override void WndProc(ref Message message)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MOVE = 0xF010;

            switch (message.Msg)
            {
                case WM_SYSCOMMAND:
                    int command = message.WParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                        return;
                    break;
            }

            base.WndProc(ref message);
        }

        public void DisplayGlassVisionDashboard(VisionStatus status)
        {
            switch (status)
            {
                case VisionStatus.OK:
                    pBoxOKGlass.Image = Properties.Resources.visionOKOn;
                    pBoxNgGlass.Image = Properties.Resources.visionNGOff;
                    break;

                case VisionStatus.NG:
                    pBoxOKGlass.Image = Properties.Resources.visionOKOff;
                    pBoxNgGlass.Image = Properties.Resources.visionNGOn;
                    break;

                case VisionStatus.NONE:
                    pBoxOKGlass.Image = Properties.Resources.visionOKOff;
                    pBoxNgGlass.Image = Properties.Resources.visionNGOff;
                    break;
            }
        }

        public void DisplayVehicleVisionDashboard(VisionStatus status)
        {
            switch (status)
            {
                case VisionStatus.OK:
                    pBoxOKVehicle.Image = Properties.Resources.visionOKOn;
                    pBoxNgVehicle.Image = Properties.Resources.visionNGOff;
                    break;

                case VisionStatus.NG:
                    pBoxOKVehicle.Image = Properties.Resources.visionOKOff;
                    pBoxNgVehicle.Image = Properties.Resources.visionNGOn;
                    break;

                case VisionStatus.NONE:
                    pBoxOKVehicle.Image = Properties.Resources.visionOKOff;
                    pBoxNgVehicle.Image = Properties.Resources.visionNGOff;
                    break;
            }
        }

        public void DisplayGapVisionDashboard(VisionStatus status)
        {
            switch (status)
            {
                case VisionStatus.OK:
                    pBoxOKGap.Image = Properties.Resources.visionOKOn;
                    pBoxNgGap.Image = Properties.Resources.visionNGOff;
                    break;

                case VisionStatus.NG:
                    pBoxOKGap.Image = Properties.Resources.visionOKOff;
                    pBoxNgGap.Image = Properties.Resources.visionNGOn;
                    break;

                case VisionStatus.NONE:
                    pBoxOKGap.Image = Properties.Resources.visionOKOff;
                    pBoxNgGap.Image = Properties.Resources.visionNGOff;
                    break;
            }
        }

        public void LoadCarInfo(string carNumber, string carName) 
        {
            tbCarNumber.Text = carNumber;
            tbCarName.Text = carName;
        }

        public void LoadCarSeqNum(string carSeqNum) 
        {
            tbCarSeq.Text = carSeqNum;
        }

        public void AddNgListViewItem(string carName, int carSeqNum, TaskStage stage) 
        {
            _ngListView.AddNgListItem(carName, carSeqNum, stage);
        }
    }
}

