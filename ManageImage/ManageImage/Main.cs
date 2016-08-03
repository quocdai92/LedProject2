using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            panel1.Height = CellSize * h;
            panel1.Width = CellSize * w;
            ResetGridPanel(CellSize);
            panel1.Invalidate();
            dgvListArea.Columns.Add("Name", "Name");
            dgvListArea.Columns.Add("Width", "Width");
            dgvListArea.Columns.Add("Height", "Height");
            dgvListArea.Columns[1].Width = 80;
            dgvListArea.Columns[2].Width = 80;
            dgvListArea.MultiSelect = false;
            dgvListArea.Rows[0].Selected = true;
            if (myBuffer != null)
                myBuffer.Dispose();
            myBuffer = currentContext.Allocate(this.panel1.CreateGraphics(),
                                               this.panel1.DisplayRectangle);
            T.Tick += Slider;
        }
        private short w = 50;
        private short h = 100;
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
            "red","orange","yellow","green","blue","violet", "black"
        };

        public static List<DisplayArea> ListAreas = new List<DisplayArea>();
        public static DisplayArea CurrentArea;
        public static DataTable ListAreaTable;
        public static int Interval = 50;
        public static Frames Frames;
        public static Timer T = new Timer()
        {
            Interval = Interval
        };
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
            if (isDrawDisplayArea && isEditable)
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
                    CurrentArea.DisplayRectangle = new Rectangle(x, y, width, height);
                    var listCellInArea = gridMaps.Where(cell => CurrentArea != null && IsGridInTheDisplayArea(cell, CurrentArea.DisplayRectangle)).ToList();
                    CurrentArea.ListGrid = listCellInArea;
                    if (dgvListArea.CurrentRow != null && dgvListArea.CurrentRow.Cells[0].Value != null)
                    {
                        //set value of width:
                        var w = getWidthOfArea(CurrentArea.ListGrid) + 1;
                        dgvListArea.CurrentRow.Cells[1].Value = w;
                        CurrentArea.Width = w;
                        //set value of height:
                        var h = getHeightOfArea(CurrentArea.ListGrid) + 1;
                        dgvListArea.CurrentRow.Cells[2].Value = h;
                        CurrentArea.Height = h;
                    }

                }
                isChangeGrid = true;
                Point changePoint = new Point(e.Location.X - startPosition.X,
                                  e.Location.Y - startPosition.Y);
                //if (panel2.AutoScrollPosition.X + changePoint.X > panel2.Width
                //    || panel2.AutoScrollPosition.Y + changePoint.Y > panel2.Height)

                    panel2.AutoScrollPosition = new Point(panel2.AutoScrollPosition.X + changePoint.X,
                                                          panel2.AutoScrollPosition.Y + changePoint.Y);

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
                    MessageBox.Show(@"You must create display area to continue.", @"Error", MessageBoxButtons.OK,
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
        private void dgvListArea_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (CurrentArea != null)
            {
                if (CurrentArea.ListGrid == null || CurrentArea.ListGrid.Count == 0)
                {
                    MessageBox.Show(@"You must completed the region before.", @"Warning", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
                else if (CurrentArea.ListFileTemplates == null || CurrentArea.ListFileTemplates.Count == 0)
                {
                    MessageBox.Show(@"You must completed the region before.", @"Warning", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
                else if (IsRegionOverlap())
                {
                    MessageBox.Show(@"Current Area is overlayed. Please select another region.", @"Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    e.Cancel = true;
                }
                else
                {
                    SaveCurrentArea(CurrentArea);
                }

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
                    MessageBox.Show(@"You must Save before.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPlay = false;
            T.Stop();
            //isEditable = true;
            //enableEditionToolStripMenuItem.Enabled = false;
            //isDrawDisplayArea = true;
            //panel1.Focus();
            //panel1.Invalidate();
            index = 0;
        }
        #endregion
        #region Button
        private void btnNewArea_Click(object sender, EventArgs e)
        {
            if (ListAreas.Count > 6)
            {
                MessageBox.Show(@"Can't add more area.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (CurrentArea != null)
                {
                    if (CurrentArea.ListGrid == null || CurrentArea.ListGrid.Count == 0)
                    {
                        MessageBox.Show(@"You must completed the region before.", @"Warning", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                    else if (CurrentArea.ListFileTemplates == null || CurrentArea.ListFileTemplates.Count == 0)
                    {
                        MessageBox.Show(@"You must completed the region before.", @"Warning", MessageBoxButtons.OK,
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (CurrentArea != null && CurrentArea.ListGrid.Count == 0)
            {
                MessageBox.Show(@"You must create display area to continue.", @"Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }
            else if (CurrentArea != null && CurrentArea.ListFileTemplates.Count == 0)
            {
                MessageBox.Show(@"You must choose effects before.", @"Warning", MessageBoxButtons.OK,
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
                CurrentArea = null;
                MessageBox.Show(@"Save Successful.", @"Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                trkbGridSize.Enabled = false;
                isEditable = false;
                enableEditionToolStripMenuItem.Enabled = true;
            }
        }
        private void btnDelete_Click(object sender, EventArgs e)
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
        private void btnEdit_Click(object sender, EventArgs e)
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
                    MessageBox.Show(@"You must create display area to continue.", @"Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
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
            for (int y = 0; y <= h; ++y)
            {
                for (int k = 0; k <= w; k++)
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
                var minX = listCells[0].StartPosition.X;
                var maxX = listCells[0].StartPosition.X;
                foreach (var cell in listCells)
                {
                    if (cell.StartPosition.X <= minX)
                    {
                        minX = cell.StartPosition.X;
                    }
                    if (cell.StartPosition.X >= maxX)
                    {
                        maxX = cell.StartPosition.X;
                    }
                }
                CurrentArea.X = minX;
                //CurrentArea.Width = maxX - minX + _cellSize;
                return (maxX - minX) / CellSize;
            }
            return 0;
        }
        private int getHeightOfArea(List<Cell> listCells)
        {
            if (listCells != null && listCells.Count > 0)
            {
                var minY = listCells[0].StartPosition.Y;
                var maxY = listCells[0].StartPosition.Y;
                foreach (var cell in listCells)
                {
                    if (cell.StartPosition.Y <= minY)
                    {
                        minY = cell.StartPosition.Y;
                    }
                    if (cell.StartPosition.Y >= maxY)
                    {
                        maxY = cell.StartPosition.Y;
                    }
                }
                CurrentArea.Y = minY;
                //CurrentArea.Height = maxY - minY + _cellSize;
                return (maxY - minY) / CellSize;
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
                    case "yellow": return new SolidBrush(Color.FromArgb(128, Color.Yellow));
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
                    case "orange": return Color.Orange;
                    case "yellow": return Color.Yellow;
                    case "green": return Color.Green;
                    case "blue": return Color.Blue;
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
                if (file.ListImages != null && file.ListImages.Count > 0)
                {
                    timeTotal += file.TimePlay;
                    var countImge = file.TimePlay * 1000 / Interval;
                    for (int i = 0; i < countImge; i++)
                    {
                        var idx = i % file.ListImages.Count;
                        listImg.Add(file.ListImages[idx]);
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
                Name = "Area " + areaId,
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
                            Frames.DisplayAreas.Add(ListAreas[i]);
                            var img = ListAreas[i].ListImages[index];
                            Bitmap bm = new Bitmap(img);
                            for (int x = 0; x < img.Width; x++)
                            {
                                for (int y = 0; y < img.Height; y++)
                                {
                                    Color color = bm.GetPixel(x, y);
                                    ListAreas[i].ListGrid[y * img.Width + x].Color = color;
                                    Frames.ListGrid.Add(ListAreas[i].ListGrid[y * img.Width + x]);
                                }
                            }
                        }
                    }
                    index++;
                }
                else
                {
                    index = 0;
                }
            }
            this.panel1.Focus();
            this.panel1.Invalidate();
        }

        private void trkbGridSize_ValueChanged(object sender, EventArgs e)
        {
            CellSize = trkbGridSize.Value;
            ResetGridPanel(CellSize);
            //isDrawDisplayArea = false;
            foreach (var area in ListAreas)
            {
                area.ListGrid = new List<Cell>();
            }
            if (myBuffer != null)
                myBuffer.Dispose();
            myBuffer = currentContext.Allocate(this.panel1.CreateGraphics(),
                                               this.panel1.DisplayRectangle);
            //panel1.Dispose();
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
            panel1.Width = CellSize*w;
            panel1.Height = CellSize*h;
            panel1.Invalidate();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (myBuffer != null)
                myBuffer.Dispose();
            myBuffer = currentContext.Allocate(this.panel1.CreateGraphics(),
                                               this.panel1.DisplayRectangle);

            panel1.Focus();
            panel1.Invalidate();
            ResetGridPanel(CellSize);
            //setup(false);
        }
    }
}
