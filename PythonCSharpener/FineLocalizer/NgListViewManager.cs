using System.Collections.Generic;
using YamlDotNet.Serialization;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using CommonUtils;
using System.ComponentModel;
using System.Windows.Forms.Design;

namespace FineLocalizer
{
    [Designer(typeof(ControlDesigner))]
    public class NgListViewManager
    {
        private List<NgListItem> _ngArr = new List<NgListItem>();
        private static readonly LogHelper Logger = LogHelper.Logger;
        private string _filePath;
        private ISerializer _serializerBuilder = new SerializerBuilder().Build();
        private IDeserializer _deserializerBuilder = new DeserializerBuilder().Build();
        private ListView _ltvNg;
        private ColumnHeader _hCarType = new ColumnHeader();
        private ColumnHeader _hCarSeqNum = new ColumnHeader();
        private ColumnHeader _hType = new ColumnHeader();
        private ColumnHeader _hDate = new ColumnHeader();

        private int _maxCount = 100;
        private Font _headerFont;
        private Font _itemFont;
        private TextFormatFlags _cFlag = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;

        public NgListViewManager(ListView ltv, float headerFontSize, float itemFontSize)
        {
            _ltvNg = ltv;
            _filePath = $"{Logger.LogPath}/NgList.yml";
            _ltvNg.DrawColumnHeader += ltvNG_DrawColumnHeader;
            _ltvNg.DrawSubItem += ltvNG_DrawSubItem;

            _ltvNg.OwnerDraw = true;
            _ltvNg.GridLines = true;
            _ltvNg.ForeColor = Color.FromArgb(255, 29, 51);
            _ltvNg.Font = new Font("Consolas", 12f);

            _headerFont = new Font(FontManager.CustomFont, headerFontSize);
            _itemFont = new Font(FontManager.CustomFont, itemFontSize);

            _hCarType.Text = Lang.FineLo.ltvHeaderCarType;
            _hCarSeqNum.Text = Lang.FineLo.ltvHeaderCarSeqNum;
            _hType.Text = Lang.FineLo.ltvHeaderType;
            _hDate.Text = Lang.FineLo.ltvHeaderDate;

            _ltvNg.Columns.AddRange(new ColumnHeader[]
            {
                _hCarType, _hCarSeqNum, _hType, _hDate
            });

            _hCarType.Width = (int)(_ltvNg.Width * 0.20);
            _hCarSeqNum.Width = (int)(_ltvNg.Width * 0.20);
            _hType.Width = (int)(_ltvNg.Width * 0.20);
            _hDate.Width = -2;

            SetHeight(_ltvNg, (int)(itemFontSize * 2));
            LoadAndShowNgListFile();
        }

        private void SetHeight(ListView listView, int height)
        {
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, height);
            listView.SmallImageList = imgList;
        }

        public void ReportAndSaveNgInfo(string carName, int carSeqNum, TaskStage stage)
        {
            AddNgListItem(carName, carSeqNum, stage);
            SaveNgListFile();
        }

        public void AddNgListItem(string carName, int carSeqNum, TaskStage stage) 
        {
            NgListItem ngItem = new NgListItem(carName, carSeqNum, stage);
            if (_ltvNg.Items.Count >= _maxCount)
            {
                _ltvNg.Items.RemoveAt(_ltvNg.Items.Count - 1);
                _ngArr.RemoveAt(_ngArr.Count - 1);
            }
            _ltvNg.Items.Insert(0, ngItem.ConvertListViewItem());
            _ngArr.Insert(0, ngItem);

            _hCarType.Width = (int)(_ltvNg.Width * 0.20);
            _hCarSeqNum.Width = (int)(_ltvNg.Width * 0.20);
            _hType.Width = (int)(_ltvNg.Width * 0.20);
            _hDate.Width = -2;
        }

        private void SaveNgListFile()
        {
            using (var stream = File.Create(_filePath))
            using (var writer = new StreamWriter(stream))
            {
                _serializerBuilder.Serialize(writer, _ngArr);
            }
        }

        private void LoadNgListFile()
        {
            using (var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                _ngArr = _deserializerBuilder.Deserialize<List<NgListItem>>(reader);
            }
        }

        public void LoadAndShowNgListFile()
        {
            _ltvNg.Items.Clear();
            if (File.Exists(_filePath))
            {
                LoadNgListFile();
                if (_ngArr != null)
                {
                    foreach (var value in _ngArr)
                    {
                        _ltvNg.Items.Add(value.ConvertListViewItem());
                    }
                }
            }
        }

        private void ltvNG_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(SystemBrushes.Menu, e.Bounds);
            e.Graphics.DrawRectangle(SystemPens.GradientInactiveCaption,
                new Rectangle(e.Bounds.X, 0, e.Bounds.Width, e.Bounds.Height));

            string text = _ltvNg.Columns[e.ColumnIndex].Text;

            TextRenderer.DrawText(e.Graphics, text, _headerFont, e.Bounds, Color.Black, _cFlag);
        }

        private void ltvNG_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, _itemFont, e.Bounds, Color.Red, _cFlag);
        }
    }
}