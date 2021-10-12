using System.Windows.Forms;

namespace FineLocalizer
{
    public enum TaskStage
    {
        Glass = 1,
        Vehicle = 2,
        Gap = 3
    }

    public class NgListItem
    {
        public string CarName { get; set; }

        public int CarSeqNum { get; set; }

        public TaskStage Stage { get; set; }

        public string Date { get; set; }

        public NgListItem (string carName, int carSeqNum, TaskStage stage)
        {
            CarName = carName;
            CarSeqNum = carSeqNum;
            Stage = stage;
            Date = $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        public NgListItem()
            : this("unknown", 0, TaskStage.Vehicle)
        {
        }

        public ListViewItem ConvertListViewItem()
        {
            ListViewItem item = new ListViewItem();
            item.Text = CarName;
            item.SubItems.Add(CarSeqNum.ToString());
            item.SubItems.Add(ConvertEnumToString(Stage));
            item.SubItems.Add(Date);

            return item;
        }

        public string ConvertEnumToString(TaskStage stage)
        {
            string str = "";
            switch (stage)
            {
                case TaskStage.Glass:
                    str = Lang.FineLo.taskStageGlass;
                    break;
                case TaskStage.Vehicle:
                    str = Lang.FineLo.taskStageVehicle;
                    break;
                case TaskStage.Gap:
                    str = Lang.FineLo.taskStageGap;
                    break;
            }
            return str;
        }
    }
}
