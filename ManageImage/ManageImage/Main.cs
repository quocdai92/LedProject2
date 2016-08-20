using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using LedFullControl;
using LedProject;

namespace ManageImage
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

            currentContext = BufferedGraphicsManager.Current;
            isDrawDisplayArea = false;
            panel1.Height = CellSize * height;
            panel1.Width = CellSize * width;
            //ResetGridPanel(CellSize);
            panel1.Invalidate();
            dgvListArea.Columns.Add("Name", ConfigurationManager.AppSettings["Name"] ?? "Name");
            dgvListArea.Columns.Add("Width", ConfigurationManager.AppSettings["Width"] ?? "Width");
            dgvListArea.Columns.Add("Height", ConfigurationManager.AppSettings["Height"] ?? "Height");
            dgvListArea.Columns[1].Width = 35;
            dgvListArea.Columns[2].Width = 35;
            dgvListArea.Columns[0].Width = 180;
            dgvListArea.MultiSelect = false;
            dgvListArea.Rows[0].Selected = true;
            dgvListArea.ReadOnly = true;
            if (myBuffer != null)
                myBuffer.Dispose();
            myBuffer = currentContext.Allocate(this.panel1.CreateGraphics(),
                                               this.panel1.DisplayRectangle);
            T.Tick += Slider;
        }
        public short width = 100;
        public short height = 100;
        public static int CellSize = 20;
        private List<Cell> gridMaps = new List<Cell>();
        private bool isDrawDisplayArea;
        private Point startPosition;
        private Point endPosition;
        private BufferedGraphics myBuffer;
        private BufferedGraphicsContext currentContext;
        private bool isDragging;
        private bool isPlay;
        private bool isEditable = true;
        private bool isChangeGrid;
        private int areaId = 1;
        private int index;
        private string[] listColor =
        {
            "red",
            "green",
            "blue",
            "orange",
            "cyan",
            "violet",
            "black"
        };

        public static List<DisplayArea> ListAreas = new List<DisplayArea>();
        public static DisplayArea CurrentArea;
        public static DataTable ListAreaTable;
        public static int Interval = 50;
        public static Frames Frames;
        public List<Frames> ListFrames;
        public List<byte> FileToSave = new List<byte>();
        //public bool isPlayAsFrame;
        public static Timer T = new Timer()
        {
            Interval = Interval
        };

        private frmMAP map;
        public int minX, minY, maxX, maxY;
        public List<Point> line1;
        public List<Point> line2;
        //public static List<FileTemplate> ListFileTemplates = new List<FileTemplate>(); 

        #region Event Handle
        #region Paint
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            //Graphics g = e.Graphics;
            Pen p = new Pen(Color.Black);
            SolidBrush brush = new SolidBrush(Color.FromArgb(128, Color.Red));
            myBuffer.Graphics.Clear(Color.Black);
            if (gridMaps != null)
            {
                foreach (var cell in gridMaps)
                {
                    myBuffer.Graphics.FillEllipse(new SolidBrush(Color.SlateGray),
                        cell.StartPosition.X, cell.StartPosition.Y, 2 * cell.Size / 3, 2 * cell.Size / 3);
                }
            }
            if ((isDrawDisplayArea && isEditable) || !isPlay)
            {
                //myBuffer.Graphics.DrawRectangle(recPen, displayRectangle);
                if (ListAreas != null && ListAreas.Count > 0)
                {
                    foreach (var displayArea in ListAreas)
                    {
                        brush = GetAreaColor(displayArea.Color);
                        if (displayArea.ListGrid != null)
                        {
                            foreach (var cell in displayArea.ListGrid)
                            {
                                myBuffer.Graphics.FillEllipse(brush, cell.StartPosition.X,
                                    cell.StartPosition.Y, 2 * cell.Size / 3, 2 * cell.Size / 3);
                            }
                        }
                    }
                }
                if (CurrentArea != null && CurrentArea.ListGrid != null)
                {
                    brush = GetAreaColor(CurrentArea.Color);
                    foreach (var cell in CurrentArea.ListGrid)
                    {
                        myBuffer.Graphics.FillEllipse(brush, cell.StartPosition.X,
                            cell.StartPosition.Y, 2 * cell.Size / 3, 2 * cell.Size / 3);
                    }
                }
            }
            if (isPlay)
            {

                if (Frames.ListGrid.Count > 0)
                {
                    foreach (var cell in Frames.ListGrid)
                    {
                        brush.Color = cell.Color;
                        myBuffer.Graphics.FillEllipse(brush, cell.StartPosition.X,
                            cell.StartPosition.Y, 2 * cell.Size / 3, 2 * cell.Size / 3);
                    }
                }
            }
            myBuffer.Render(this.panel1.CreateGraphics());
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDrawDisplayArea && isEditable)
            {
                startPosition = e.Location;
                isDragging = true;
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                endPosition = e.Location;
                var width = Math.Abs(endPosition.X - startPosition.X);
                var height = Math.Abs(endPosition.Y - startPosition.Y);
                var x = startPosition.X;
                var y = startPosition.Y;
                if (endPosition.X < startPosition.X && endPosition.Y < startPosition.Y)
                {
                    x = endPosition.X;
                    y = endPosition.Y;
                }
                else if (endPosition.X < startPosition.X && endPosition.Y > startPosition.Y)
                {
                    x = endPosition.X;
                    y = startPosition.Y;
                }
                else if (endPosition.X > startPosition.X && endPosition.Y < startPosition.Y)
                {
                    x = startPosition.X;
                    y = endPosition.Y;
                }
                if (CurrentArea != null)
                {
                    var area = new Rectangle(x, y, width, height);
                    var listCellInArea = gridMaps.Where(cell => CurrentArea != null && IsGridInTheDisplayArea(cell, area)).ToList();
                    listCellInArea.RemoveAll(m => ListAreas.Any(a => a.ListGrid.Contains(m) && a.AreaId != CurrentArea.AreaId));
                    if (ModifierKeys.HasFlag(Keys.Control))
                    {
                        foreach (var cell in listCellInArea)
                        {
                            if (!CurrentArea.ListGrid.Contains(cell))
                            {
                                CurrentArea.ListGrid.Add(cell);
                            }
                        }

                    }
                    else
                    {
                        CurrentArea.ListGrid = listCellInArea;
                    }
                    if (dgvListArea.CurrentRow != null && dgvListArea.CurrentRow.Cells[0].Value != null)
                    {
                        //set value of width:
                        var w = getWidthOfArea(CurrentArea.ListGrid);
                        dgvListArea.CurrentRow.Cells[1].Value = w;
                        //CurrentArea.Width = w;
                        //set value of height:
                        var h = getHeightOfArea(CurrentArea.ListGrid);
                        dgvListArea.CurrentRow.Cells[2].Value = h;
                        //CurrentArea.Height = h;
                    }

                }
                isChangeGrid = true;
                if (cbkAutoScroll.Checked)
                {
                    Point changePoint = new Point(e.Location.X - startPosition.X,
                                  e.Location.Y - startPosition.Y);
                    if (e.Location.X < panel1.Width / 3 || e.Location.X > 2 * panel2.Width / 3 ||
                        e.Location.Y < panel1.Height / 3 || e.Location.Y > 2 * panel2.Height / 3)
                    {
                        panel2.AutoScrollPosition = new Point(panel2.AutoScrollPosition.X + changePoint.X / 2,
                                                              panel2.AutoScrollPosition.Y + changePoint.Y / 2);
                    }
                }
                panel1.Invalidate();
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                endPosition = e.Location;
                isDragging = false;
            }
        }
        #endregion
        #region dgvListArea
        private void dgvListArea_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (CurrentArea != null && isEditable)
            {
                if (CurrentArea.Width > 0 && CurrentArea.Height > 0)
                {

                    CurrentArea.ListImages = new List<Image>();
                    FormEdit frm1 = new FormEdit(CurrentArea);
                    frm1.ShowDialog();
                }
                else
                {
                    MessageBox.Show(@"Bạn Phải tạo vùng hiển thị trước...", @"Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
        private void dgvListArea_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvListArea.CurrentRow != null && dgvListArea.CurrentRow.Cells[0].Value != null)
            {
                isDrawDisplayArea = true;
                var id = Regex.Match(dgvListArea.CurrentRow.Cells[0].Value.ToString(), @"\d+").Value;
                CurrentArea = ListAreas.FirstOrDefault(m => m.AreaId == Convert.ToInt32(id));
            }
            else
            {
                CurrentArea = null;
                isDrawDisplayArea = false;
            }

        }

        #endregion
        #region Menu
        private void enableEditionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isEditable)
            {
                isDrawDisplayArea = true;
                isEditable = true;
                enableEditionToolStripMenuItem.Enabled = false;
                trkbGridSize.Enabled = true;
                if (dgvListArea.CurrentRow != null && dgvListArea.CurrentRow.Cells[0].Value != null)
                {
                    isDrawDisplayArea = true;
                    var id = Regex.Match(dgvListArea.CurrentRow.Cells[0].Value.ToString(), @"\d+").Value;
                    CurrentArea = ListAreas.FirstOrDefault(m => m.AreaId == Convert.ToInt32(id));
                }
            }
        }
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnSave_Click(sender, e);
            if (ListAreas.Count > 0)
            {
                if (CurrentArea == null)
                {
                    T.Start();
                    isPlay = true;
                    trkbGridSize.Enabled = false;
                }
                else
                {
                    MessageBox.Show(@"Làm ơn Save giùm...", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPlay = false;
            T.Stop();
            index = 0;
            panel1.Invalidate();
        }
        #endregion
        #region Button
        private void btnNewArea_Click(object sender, EventArgs e)
        {
            if (isEditable)
            {
                if (ListAreas.Count > 5)
                {
                    MessageBox.Show(@"Không thể thêm nhiều vùng hơn nữa.", @"Có quá nhiều vùng.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (CurrentArea != null)
                    {
                        if (CurrentArea.ListGrid == null || CurrentArea.ListGrid.Count == 0)
                        {
                            MessageBox.Show(@"Vẽ vùng hiển thị nào...", @"Không có vùng hiển thị", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                        else if (CurrentArea.ListFileTemplates == null || CurrentArea.ListFileTemplates.Count == 0)
                        {
                            MessageBox.Show(@"Thêm hiệu ứng cho vùng trước đi..", @"Thiếu hiệu ứng..", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                        else
                        {
                            SaveCurrentArea(CurrentArea);
                            AddNewArea();
                        }
                    }
                    else
                    {
                        AddNewArea();
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (isEditable)
            {
                if (CurrentArea != null && CurrentArea.ListGrid.Count == 0)
                {
                    MessageBox.Show(@"Tạo vùng hiển thị trước đi nào..", @"Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                }
                else if (CurrentArea != null && CurrentArea.ListFileTemplates.Count == 0)
                {
                    MessageBox.Show(@"Chọn hiệu ứng trước khi lưu...", @"Warning", MessageBoxButtons.OK,
                                 MessageBoxIcon.Warning);
                }
                else if (CurrentArea != null && CurrentArea.ListFileTemplates.Count > 0)
                {
                    isDragging = false;
                    isDrawDisplayArea = false;
                    //calculate time play:
                    var time = ListAreas.Max(m => m.TimePlay);

                    foreach (var area in ListAreas)
                    {
                        area.TimePlay = time;
                        if (area.ListFileTemplates != null && area.ListFileTemplates.Count > 0)
                        {
                            area.ListImages = GetListImageOfArea(area.ListFileTemplates, time);
                        }
                    }
                    SaveCurrentArea(CurrentArea);

                    //add new area black:
                    DisplayArea newArea = new DisplayArea();
                    foreach (var gridMap in gridMaps)
                    {
                        if (!ListAreas.Any(m => m.ListGrid.Contains(gridMap)))
                        {
                            var grid = new Cell()
                            {
                                IsEmpty = true,
                                Color = Color.Black,
                                X = gridMap.X,
                                Y = gridMap.Y
                            };
                            newArea.ListGrid.Add(grid);
                        }
                    }
                    ListAreas.Add(newArea);
                    CurrentArea = null;
                    //MessageBox.Show(@"Save hoàn thành...", @"Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    trkbGridSize.Enabled = false;
                    isEditable = false;
                    enableEditionToolStripMenuItem.Enabled = true;
                    ptbEnableEdit.Enabled = true;
                }
            }
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (isEditable)
            {
                if (CurrentArea != null)
                {
                    var x = new List<DisplayArea>();
                    x = ListAreas.Where(m => m.AreaId != CurrentArea.AreaId).ToList();
                    //var idx = ListAreas.FindIndex(m => m.AreaId == CurrentArea.AreaId);
                    //ListAreas.RemoveAt(idx);
                    ListAreas = new List<DisplayArea>();
                    ListAreas.AddRange(x);
                    CurrentArea = null;
                    if (dgvListArea.CurrentRow != null)
                        dgvListArea.Rows.RemoveAt(dgvListArea.CurrentRow.Index);
                    isDrawDisplayArea = true;
                    //panel1.Dispose();
                    if (myBuffer != null)
                    {
                        myBuffer.Dispose();
                        myBuffer = currentContext.Allocate(this.panel1.CreateGraphics(),
                                                           this.panel1.DisplayRectangle);
                    }
                    panel1.Invalidate();
                }
            }
        }
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (isEditable)
            {
                if (map == null)
                {
                    map = new frmMAP();
                }
                map.ShowDialog();
            }
        }
        #endregion
        #endregion

        #region Private Method
        private bool IsGridInTheDisplayArea(Cell grid, Rectangle displayRectangle)
        {
            if (IsPointInTheRectangle(grid.StartPosition.X, grid.StartPosition.Y, displayRectangle)
                || IsPointInTheRectangle(grid.StartPosition.X + grid.Size, grid.StartPosition.Y, displayRectangle)
                || IsPointInTheRectangle(grid.StartPosition.X, grid.StartPosition.Y + grid.Size, displayRectangle)
                || IsPointInTheRectangle(grid.StartPosition.X + grid.Size, grid.StartPosition.Y + grid.Size,
                    displayRectangle))
                return true;
            return false;
        }

        private bool IsPointInTheRectangle(int x, int y, Rectangle displayRectangle)
        {
            if (displayRectangle.Width == 0 || displayRectangle.Height == 0)
            {
                return false;
            }
            if (x >= displayRectangle.X && x <= displayRectangle.X + displayRectangle.Width
                && y >= displayRectangle.Y && y <= displayRectangle.Y + displayRectangle.Height)
            {
                return true;
            }
            return false;
        }
        private void ResetGridPanel(int cellSize)
        {
            gridMaps = new List<Cell>();
            for (int y = 0; y < height; ++y)
            {
                for (int k = 0; k < width; k++)
                {
                    var cell = new Cell()
                    {
                        Size = cellSize,
                        StartPosition = new Point(k * cellSize, y * cellSize),
                        X = k,
                        Y = y
                    };
                    gridMaps.Add(cell);
                }
            }
        }

        private int getWidthOfArea(List<Cell> listCells)
        {
            if (listCells != null && listCells.Count > 0)
            {
                var minX = listCells.Min(m => m.X);
                var maxX = listCells.Max(m => m.X);
                CurrentArea.Width = maxX - minX + 1;
                CurrentArea.X = minX;
                return CurrentArea.Width;
            }
            return 0;
        }
        private int getHeightOfArea(List<Cell> listCells)
        {
            if (listCells != null && listCells.Count > 0)
            {
                var minY = listCells.Min(m => m.Y);
                var maxY = listCells.Max(m => m.Y);
                CurrentArea.Height = maxY - minY + 1;
                CurrentArea.Y = minY;
                return CurrentArea.Height;
            }
            return 0;
        }

        private void SaveCurrentArea(DisplayArea displayArea)
        {
            if (displayArea != null && displayArea.ListImages.Count == 0)
            {
                if (ListAreas.Any(m => m.AreaId == displayArea.AreaId))
                {
                    var area = ListAreas.FirstOrDefault(m => m.AreaId == displayArea.AreaId);
                    area = displayArea;
                    var time = ListAreas.Max(m => m.TimePlay);
                    area.TimePlay = time;
                    if (area.ListFileTemplates != null && area.ListFileTemplates.Count > 0)
                    {
                        area.ListImages = GetListImageOfArea(area.ListFileTemplates, time);
                    }

                }
                else
                {
                    ListAreas.Add(displayArea);
                }
            }
            else
            {
                //save Image to new grid:
                var lstImageNew = new List<Image>();
                foreach (var image in CurrentArea.ListImages)
                {
                    var destRect = new Rectangle(0, 0, CurrentArea.Width, CurrentArea.Height);
                    var destImage = new Bitmap(CurrentArea.Width, CurrentArea.Height);

                    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                    using (var graphics = Graphics.FromImage(destImage))
                    {
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        using (var wrapMode = new ImageAttributes())
                        {
                            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                        }
                    }
                    lstImageNew.Add(destImage);
                }
                CurrentArea.ListImages = new List<Image>(lstImageNew);
            }
        }

        private SolidBrush GetAreaColor(string color)
        {
            if (color != null)
            {
                switch (color)
                {
                    case "red": return new SolidBrush(Color.FromArgb(128, Color.Red));
                    case "orange": return new SolidBrush(Color.FromArgb(128, Color.Orange));
                    case "cyan": return new SolidBrush(Color.FromArgb(128, Color.Cyan));
                    case "green": return new SolidBrush(Color.FromArgb(128, Color.Green));
                    case "blue": return new SolidBrush(Color.FromArgb(128, Color.Blue));
                    case "violet": return new SolidBrush(Color.FromArgb(128, Color.Violet));
                }
            }
            return new SolidBrush(Color.FromArgb(128, Color.Black));
        }
        private Color GetTextColor(string color)
        {
            if (color != null)
            {
                switch (color)
                {
                    case "red": return Color.Red;
                    case "green": return Color.Green;
                    case "blue": return Color.Blue;
                    case "orange": return Color.Orange;
                    case "cyan": return Color.Cyan;
                    case "violet": return Color.Violet;
                }
            }
            return Color.Black;
        }

        private List<Image> GetListImageOfArea(List<FileTemplate> listFile, int timeMax)
        {
            var listImg = new List<Image>();
            int timeTotal = 0;
            foreach (var file in listFile)
            {
                if (file.ListImageReturn != null && file.ListImageReturn.Count > 0)
                {
                    timeTotal += file.TimePlay;
                    var countImge = file.TimePlay * 1000 / Interval;
                    for (int i = 0; i < countImge; i++)
                    {
                        var idx = i % file.ListImageReturn.Count;
                        listImg.Add(file.ListImageReturn[idx]);
                    }
                }
            }
            var result = new List<Image>();
            var totalImage = timeMax * 1000 / Interval;
            for (int i = 0; i < totalImage; i++)
            {
                var idx = i % listImg.Count;
                result.Add(listImg[idx]);
            }
            return result;
        }

        private string GetColorOfNewArea()
        {
            var lstColor = new List<string>();
            if (ListAreas.Count > 0)
            {
                lstColor.AddRange(ListAreas.Select(area => area.Color));
                var x = listColor.Except(lstColor).ToList();
                if (x.Count > 0)
                {
                    return x[0];
                }
            }
            else
            {
                return "red";
            }
            return "black";
        }

        private void AddNewArea()
        {
            var area = new DisplayArea()
            {
                AreaId = areaId,
                Name = ConfigurationManager.AppSettings["NameArea"] == null ? "Area" : ConfigurationManager.AppSettings["NameArea"] + " " + areaId,
                Color = GetColorOfNewArea()
            };
            var idx = dgvListArea.Rows.Add(area.Name, "0", "0");
            dgvListArea.Rows[idx].Cells[0].Style = new DataGridViewCellStyle
            {
                ForeColor = GetTextColor(area.Color)
            };
            dgvListArea.Rows[idx].HeaderCell.Value = String.Format("{0}", idx + 1);
            ListAreas.Add(area);
            areaId++;
        }
        private bool IsRegionOverlap()
        {
            if (CurrentArea != null)
            {
                var otherArea = ListAreas.Where(m => m.AreaId != CurrentArea.AreaId).ToList();
                foreach (var area in otherArea)
                {
                    if (area.ListGrid != null)
                    {
                        var result = CurrentArea.ListGrid.Intersect(area.ListGrid).ToList();
                        if (result.Count > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        private void Slider(Object source, EventArgs e)
        {
            Frames = new Frames()
            {
                ListImages = new List<Image>(),
                DisplayAreas = new List<DisplayArea>(),
                ListGrid = new List<Cell>()
            };
            //var id = ListAreas.Max(m => m.ListImages.Count);
            var ii = ListAreas.FindIndex(m => m.ListImages.Count > 0);
            if (ListAreas.Count > 0)
            {
                if (ii >= 0 && index < ListAreas[ii].ListImages.Count)
                {
                    for (int i = 0; i < ListAreas.Count; i++)
                    {
                        if (ListAreas[i].AreaId > 0 && ListAreas[i].ListGrid.Count > 0 && ListAreas[i].ListImages.Count > 0)
                        {
                            //Frames.DisplayAreas.Add(ListAreas[i]);
                            var img = ListAreas[i].ListImages[index];
                            Bitmap bm = new Bitmap(img);
                            foreach (var cell in ListAreas[i].ListGrid)
                            {
                                var color = bm.GetPixel(cell.X - ListAreas[i].X, cell.Y - ListAreas[i].Y);
                                cell.Color = color;
                                Frames.ListGrid.Add(cell);
                            }
                        }
                    }

                    index++;
                }
                else
                {
                    index = 0;
                    //T.Stop();
                }
            }
            //this.panel1.Focus();
            this.panel1.Invalidate();
        }

        private void trkbGridSize_ValueChanged(object sender, EventArgs e)
        {
            CellSize = trkbGridSize.Value;
            if (map != null)
            {
                width = (short)(maxX - minX + 1);
                height = (short)(maxY - minY + 1);
                //panel1.Height = CellSize * h;
                //panel1.Width = CellSize * w;
                ResetGridPanel(CellSize);
                if (line1 != null)
                {
                    if (line2 != null)
                    {
                        if (line2.Count > line1.Count)
                        {
                            //draw line2:
                            var newList = new List<Cell>();
                            foreach (var point in line2)
                            {
                                var cells = gridMaps.Where(m => m.X == point.X - minX && m.Y == point.Y - minY).ToList();
                                newList.AddRange(cells);
                            }
                            gridMaps.Clear();
                            gridMaps.AddRange(newList);
                        }
                        else
                        {
                            //draw line1:
                            var newList = new List<Cell>();
                            foreach (var point in line1)
                            {
                                var cells = gridMaps.Where(m => m.X == point.X - minX && m.Y == point.Y - minY).ToList();
                                newList.AddRange(cells);
                            }
                            gridMaps.Clear();
                            gridMaps.AddRange(newList);
                        }
                    }
                    else
                    {
                        //draw line1:
                        var newList = new List<Cell>();
                        foreach (var point in line1)
                        {
                            var cells = gridMaps.Where(m => m.X == point.X - minX && m.Y == point.Y - minY).ToList();
                            newList.AddRange(cells);
                        }
                        gridMaps.Clear();
                        gridMaps.AddRange(newList);
                    }
                }
            }
            else
            {
                ResetGridPanel(CellSize);
            }
            foreach (var area in ListAreas)
            {
                area.ListGrid = UpdateListGrid(area.ListGrid);
            }
            if (dgvListArea.CurrentRow != null && dgvListArea.CurrentRow.Cells[0].Value != null)
            {
                isDrawDisplayArea = true;
                var id = Regex.Match(dgvListArea.CurrentRow.Cells[0].Value.ToString(), @"\d+").Value;
                CurrentArea = ListAreas.FirstOrDefault(m => m.AreaId == Convert.ToInt32(id));
            }
            else
            {
                CurrentArea = null;
                isDrawDisplayArea = false;
            }
            panel1.Invalidate();
        }

        private List<Cell> UpdateListGrid(List<Cell> listGrid)
        {
            foreach (var cell in listGrid)
            {
                cell.Size = CellSize;
                cell.StartPosition = new Point(cell.X * CellSize, cell.Y * CellSize);
            }
            return listGrid;
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (myBuffer != null)
                myBuffer.Dispose();
            myBuffer = currentContext.Allocate(this.panel1.CreateGraphics(),
                                               this.panel1.DisplayRectangle);

            panel1.Focus();
            panel1.Invalidate();
            //panel2.Anchor = AnchorStyles.Bottom|AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;
            //ResetGridPanel(CellSize);
            //setup(false);
        }

        private void panel2_Scroll(object sender, ScrollEventArgs e)
        {
            panel1.Invalidate();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportData();
            //isPlayAsFrame = true;
        }

        #region Public method

        public void ReSizePanel()
        {
            width = (short)(maxX - minX + 1);
            height = (short)(maxY - minY + 1);
            if (width * height < gridMaps.Count)
            {
                foreach (var displayArea in ListAreas)
                {
                    displayArea.ListGrid.Clear();
                }
            }

            ResetGridPanel(CellSize);
            if (line1 != null)
            {
                if (line2 != null)
                {
                    if (line2.Count > line1.Count)
                    {
                        //draw line2:
                        var newList = new List<Cell>();
                        foreach (var point in line2)
                        {
                            var cells = gridMaps.Where(m => m.X == point.X - minX && m.Y == point.Y - minY).ToList();
                            newList.AddRange(cells);
                        }
                        gridMaps.Clear();
                        gridMaps.AddRange(newList);
                    }
                    else
                    {
                        //draw line1:
                        var newList = new List<Cell>();
                        foreach (var point in line1)
                        {
                            var cells = gridMaps.Where(m => m.X == point.X - minX && m.Y == point.Y - minY).ToList();
                            newList.AddRange(cells);
                        }
                        gridMaps.Clear();
                        gridMaps.AddRange(newList);
                    }
                }
                else
                {
                    //draw line1:
                    var newList = new List<Cell>();
                    foreach (var point in line1)
                    {
                        var cells = gridMaps.Where(m => m.X == point.X - minX && m.Y == point.Y - minY).ToList();
                        newList.AddRange(cells);
                    }
                    gridMaps.Clear();
                    gridMaps.AddRange(newList);
                }
            }
            lblMapWidthValue.Text = width.ToString();
            lblMapHeightValue.Text = height.ToString();
            lblMapTotalLedValue.Text = gridMaps.Count.ToString();
            panel1.Invalidate();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            startToolStripMenuItem.PerformClick();
        }

        private void btnEnableEdit_Click(object sender, EventArgs e)
        {
            enableEditionToolStripMenuItem.PerformClick();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            exportToolStripMenuItem.PerformClick();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            stopToolStripMenuItem.PerformClick();
        }

        private void ptbAddNew_MouseEnter(object sender, EventArgs e)
        {
            ptbAddNew.BackColor = Color.LightBlue;
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(ptbAddNew, "Thêm Vùng");
        }

        private void ptbAddNew_MouseLeave(object sender, EventArgs e)
        {
            ptbAddNew.BackColor = DefaultBackColor;
        }

        private void ptbMap_MouseEnter(object sender, EventArgs e)
        {
            ptbMap.BackColor = Color.LightBlue;
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(ptbMap, "Chỉnh Sửa Map");
        }

        private void ptbMap_MouseLeave(object sender, EventArgs e)
        {
            ptbMap.BackColor = DefaultBackColor;
        }

        private void ptbSave_MouseEnter(object sender, EventArgs e)
        {
            ptbSave.BackColor = Color.LightBlue;
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(ptbSave, "Save");
        }

        private void ptbSave_MouseLeave(object sender, EventArgs e)
        {
            ptbSave.BackColor = DefaultBackColor;
        }

        private void ptbDelete_MouseEnter(object sender, EventArgs e)
        {
            ptbDelete.BackColor = Color.LightBlue;
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(ptbDelete, "Xóa Vùng");
        }

        private void ptbDelete_MouseLeave(object sender, EventArgs e)
        {
            ptbDelete.BackColor = DefaultBackColor;
        }

        private void ptbPlay_MouseEnter(object sender, EventArgs e)
        {
            ptbPlay.BackColor = Color.LightBlue;
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(ptbPlay, "Play");
        }

        private void ptbPlay_MouseLeave(object sender, EventArgs e)
        {
            ptbPlay.BackColor = DefaultBackColor;
        }

        private void ptbPause_MouseEnter(object sender, EventArgs e)
        {
            ptbPause.BackColor = Color.LightBlue;
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(ptbPause, "Pause");
        }

        private void ptbPause_MouseLeave(object sender, EventArgs e)
        {
            ptbPause.BackColor = DefaultBackColor;
        }

        private void ptbExport_MouseEnter(object sender, EventArgs e)
        {
            ptbExport.BackColor = Color.LightBlue;
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(ptbExport, "Xuất File");
        }

        private void ptbExport_MouseLeave(object sender, EventArgs e)
        {
            ptbExport.BackColor = DefaultBackColor;
        }

        private void ptbEnableEdit_MouseEnter(object sender, EventArgs e)
        {
            if (isEditable)
            {
                ptbEnableEdit.BackColor = Color.LightBlue;
            }
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(ptbEnableEdit, "Cho Phép Chỉnh Sửa");
        }

        private void ptbEnableEdit_MouseLeave(object sender, EventArgs e)
        {
            ptbEnableEdit.BackColor = DefaultBackColor;
        }

        public void ClearMap()
        {
            gridMaps.Clear();
            foreach (var displayArea in ListAreas)
            {
                displayArea.ListGrid.Clear();
            }
            panel1.Invalidate();
        }

        public void ExportData()
        {
            //int idx;
            FileToSave = new List<byte>();
            ListFrames = new List<Frames>();
            var maxFrame = ListAreas.Max(m => m.ListImages.Count);
            FrmProgressbar progressbar = new FrmProgressbar();
            progressbar.progressBar1.Maximum = maxFrame;
            progressbar.progressBar1.Minimum = 0;
            progressbar.progressBar1.Step = 1;
            progressbar.Show();
            for (int idx = 0; idx < maxFrame; idx++)
            {
                //foreach (var gridMap in gridMaps)
                //{
                //    var area = ListAreas.FirstOrDefault(m => m.ListGrid.Contains(gridMap));
                //    if (area != null)
                //    {
                //        Bitmap bm = new Bitmap(area.ListImages[idx]);

                //        Bitmap bmp = new Bitmap(area.ListImages[idx]);
                //        LockBitmap lockBitmap = new LockBitmap(bmp);
                //        lockBitmap.LockBits();
                //        var color = lockBitmap.GetPixel(gridMap.X - area.X, gridMap.Y - area.Y);
                //        //gridMap.Color = color;
                //        FileToSave.Add(color.R);
                //        FileToSave.Add(color.G);
                //        FileToSave.Add(color.B);
                //        lockBitmap.UnlockBits();
                //    }
                //    else
                //    {
                //        FileToSave.Add(0);
                //        FileToSave.Add(0);
                //        FileToSave.Add(0);
                //    }
                //}
                var frame = new Frames()
                {
                    ListGrid = new List<Cell>()
                };
                for (int i = 0; i < ListAreas.Count; i++)
                {
                    var img = ListAreas[i].ListImages.Count > 0 ? ListAreas[i].ListImages[idx] : null;
                    Bitmap bm = img != null ? new Bitmap(img) : null;
                    //Frames.DisplayAreas.Add(ListAreas[i]);
                    foreach (var cell in ListAreas[i].ListGrid)
                    {
                        if (cell.IsEmpty || bm == null)
                        {
                            var grid = new Cell
                            {
                                Color = cell.Color,
                                X = cell.X,
                                Y = cell.Y
                            };
                            //cell.Color = color;
                            frame.ListGrid.Add(grid);
                        }
                        else
                        {
                            var color = bm.GetPixel(cell.X - ListAreas[i].X, cell.Y - ListAreas[i].Y);
                            var grid = new Cell
                            {
                                Color = color,
                                X = cell.X,
                                Y = cell.Y
                            };
                            //cell.Color = color;
                            frame.ListGrid.Add(grid);
                        }
                    }
                }
                ListFrames.Add(frame);

            }
            var numberLed = BitConverter.GetBytes(gridMaps.Count);
            var listByte = new List<byte>();
            listByte.AddRange(numberLed);
            foreach (var frame in ListFrames)
            {
                if (progressbar.IsCancel)
                {
                    return;
                }
                progressbar.progressBar1.PerformStep();
                frame.ListGrid = ReOrderPixcel(frame.ListGrid);
                foreach (var cell in frame.ListGrid)
                {
                    FileToSave.Add(cell.Color.R);
                    FileToSave.Add(cell.Color.G);
                    FileToSave.Add(cell.Color.B);
                }
            }
            listByte.AddRange(FileToSave);
            var dataArray = listByte.ToArray();
            var path = ConfigurationManager.AppSettings["SavePath"] ?? @"D:\Dai\Template\";
            Directory.CreateDirectory(path);
            var fileName = ConfigurationManager.AppSettings["SaveFileName"] ?? "sd.anc";
            using (FileStream
            fileStream = new FileStream(path + fileName, FileMode.Create))
            {
                // Write the data to the file, byte by byte.
                for (int i = 0; i < dataArray.Length; i++)
                {
                    fileStream.WriteByte(dataArray[i]);
                }

            }
            MessageBox.Show(@"Lưu hoàn thành...");
            progressbar.Close();
        }

        public List<Cell> ReOrderPixcel(List<Cell> listCell)
        {
            var retunValue = new List<Cell>();
            for (int i = 0; i < height; i++)
            {
                if (i % 2 == 0)
                {
                    for (int j = 0; j < width; j++)
                    {
                        var temp = listCell.FirstOrDefault(m => m.X == j && m.Y == i);
                        retunValue.Add(temp);
                    }
                }
                else
                {
                    for (int j = width - 1; j >= 0; j--)
                    {
                        var temp = listCell.FirstOrDefault(m => m.X == j && m.Y == i);
                        retunValue.Add(temp);
                    }
                }
            }
            return retunValue;
        }

        #endregion
    }
}
