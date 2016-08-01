using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LedProject;
using System.Drawing.Drawing2D;

namespace ManageImage
{
    public partial class FormEdit : Form
    {
        public class overRidePanel : Panel
        {
            protected override void OnPaintBackground(PaintEventArgs pevent) { }
        }

        private static int interval = 50;
        public static Timer T = new Timer()
        {
            Interval = interval
        };
        public string Key = "ledproject";
        private int x = 0;
        private int y = 0;
        private int width = 0;
        private int height = 0;
        public static DataTable Table = new DataTable()
        {
            Columns = { "FileName", "TimePlay (s)" }
        };
        Bitmap bitmap;
        BufferedGraphicsContext currentContext;
        BufferedGraphics myBuffer;
        PointF viewPortCenter;
        bool draging;
        Point lastMouse;
        private List<Image> ListImage;
        private List<FileTemplate> listFile = new List<FileTemplate>();
        private int index;
        private int widthShow;
        private int heightShow;
        private Image firstImage;
        private bool isStart;
        private int cellSize = Main.CellSize;
        public FormEdit(DisplayArea area)
        {
            widthShow = area.Width * cellSize;
            heightShow = area.Height * cellSize;
            Table.Rows.Clear();
            if (area.ListFileTemplates != null && area.ListFileTemplates.Count > 0)
            {
                foreach (var file in area.ListFileTemplates)
                {
                    if (bitmap == null)
                    {
                        var listImg = readFileTmp(file.FileName);
                        if (listImg.Count > 0)
                        {
                            firstImage = listImg.ElementAt(0);
                            bitmap = (Bitmap)firstImage;
                            height = heightShow;
                            width = widthShow;
                            //setup(true);
                            //setFileInfo();
                            //panel1.Invalidate();

                        }
                    }
                    Table.Rows.Add(new object[] { file.FileName, file.TimePlay });
                }
            }
            if (area.ListImages != null)
            {
                ListImage = area.ListImages;
            }
            InitializeComponent();
            currentContext = BufferedGraphicsManager.Current;
            x = panel1.Width / 2;
            y = panel1.Height / 2;
            dataGridView1.DataSource = Table;
            dataGridView1.Columns[0].Width = (int)dataGridView1.Width * 2 / 3;
            dataGridView1.Columns[1].Width = (int)dataGridView1.Width / 3;
            dataGridView1.ReadOnly = false;
            dataGridView1.Columns[0].ReadOnly = true;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
            }
            setup(true);
            if (bitmap != null)
                setFileInfo();
            T.Tick += slider;
        }

        private void setup(bool resetViewport)
        {
            if (myBuffer != null)
                myBuffer.Dispose();
            myBuffer = currentContext.Allocate(this.panel1.CreateGraphics(),
                                               this.panel1.DisplayRectangle);
            if (bitmap != null)
            {
                if (resetViewport)
                {
                    if (width == 0 || height == 0)
                        SetViewPort(new RectangleF(x - bitmap.Width / 2.0f, y - bitmap.Height / 2.0f, bitmap.Width, bitmap.Height));
                    else
                    {
                        SetViewPort(new RectangleF(x - width / 2.0f, y - height / 2.0f, width, height));
                    }
                }
            }
            this.panel1.Focus();
            this.panel1.Invalidate();
        }
        private void SetViewPort(RectangleF worldCords)
        {
            viewPortCenter = new PointF(worldCords.X + (worldCords.Width / 2.0f),
                                        worldCords.Y + (worldCords.Height / 2.0f));

        }

        private void PaintImage(int widthShow, int heightShow, int width, int height)
        {
            Rectangle drawRect = new Rectangle(
                (int)(viewPortCenter.X - width / 2.0f),
                (int)(viewPortCenter.Y - height / 2.0f),
                (int)(width),
                (int)(height));
            //this.toolStripStatusLabel1.Text = "DrawRect = " + drawRect.ToString();

            Rectangle showRect = new Rectangle((int)(panel1.Width / 2.0f - widthShow / 2.0f),
                (int)(panel1.Height / 2.0f - heightShow / 2.0f), widthShow, heightShow);

            myBuffer.Graphics.Clear(Color.White); //Clear the Back buffer
                                                  //Draw the image, Write image to back buffer, and render back buffer              
            Pen pen = new Pen(Color.Black);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            if (bitmap != null)
            {
                myBuffer.Graphics.DrawImage(bitmap, drawRect);
            }
            myBuffer.Graphics.DrawRectangle(pen, showRect);
            Brush brush;


            brush = new SolidBrush(Color.FromArgb(200, 128, 128, 128));

            pen.Color = Color.DimGray;
            var linesX = showRect.Height / (cellSize) + 1;
            var linesY = showRect.Width / (cellSize) + 1;
            //draw x
            for (int k = 0; k < linesX; k++)
            {
                myBuffer.Graphics.DrawLine(pen, showRect.X, showRect.Y + k * cellSize, showRect.X + showRect.Width, showRect.Y + k * cellSize);
            }
            //draw y
            for (int k = 0; k < linesY; k++)
            {
                myBuffer.Graphics.DrawLine(pen, showRect.X + k * cellSize, showRect.Y, showRect.X + k * cellSize, showRect.Y + showRect.Height);
            }
            myBuffer.Graphics.FillRectangle(brush, 0, 0, panel1.Width / 2 - widthShow / 2, panel1.Height);
            myBuffer.Graphics.FillRectangle(brush, panel1.Width / 2 + widthShow / 2, 0, panel1.Width / 2 - widthShow / 2, panel1.Height);
            myBuffer.Graphics.FillRectangle(brush, panel1.Width / 2 - widthShow / 2, 0, widthShow, panel1.Height / 2 - heightShow / 2);
            myBuffer.Graphics.FillRectangle(brush, panel1.Width / 2 - widthShow / 2, panel1.Height / 2 + heightShow / 2, widthShow, panel1.Height / 2 - heightShow / 2);

            myBuffer.Render(this.panel1.CreateGraphics());

        }

        public List<Image> readFileTmp(string filename)
        {
            List<Image> listImg = new List<Image>();

            var fileTobyte = File.ReadAllBytes(filename);
            var readFile = EncDec.Decrypt(fileTobyte, Key);
            int from = 0;
            for (int i = 0; i < readFile.Length; i++)
            {
                //read header:
                if ((char)readFile[i] == 'e' && (char)readFile[i + 1] == 'h' && (char)readFile[i + 2] == 'd'
                    && (char)readFile[i + 3] == 'e' && (char)readFile[i + 4] == 'r')
                {
                    from = i + 5;
                }
                //read ListImage
                if ((char)readFile[i] == 'e' && (char)readFile[i + 1] == 'n' && (char)readFile[i + 2] == 'd')
                {
                    var outFile = new byte[i - from];
                    var k = 0;
                    for (int j = from; j < i; j++)
                    {
                        outFile[k] = readFile[j];
                        k++;
                    }
                    var img = ByteArrayToImage(outFile);
                    listImg.Add(img);
                    from = i + 3;
                }
            }

            return listImg;
        }

        private void openFile()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (var fileName in openFileDialog1.FileNames)
                {
                    if (bitmap == null)
                    {
                        var listImg = readFileTmp(fileName);
                        ListImage.AddRange(listImg);
                        if (listImg.Count > 0)
                        {
                            firstImage = listImg.ElementAt(0);
                            bitmap = (Bitmap)firstImage;
                            x = panel1.Width / 2;
                            y = panel1.Height / 2;
                            height = heightShow;
                            width = widthShow;
                            setup(true);
                            setFileInfo();
                            panel1.Invalidate();

                        }
                    }
                    Table.Rows.Add(new object[] { fileName, 10 });
                }
                dataGridView1.DataSource = Table;
                dataGridView1.Columns[0].Width = (int)dataGridView1.Width * 2 / 3;
                dataGridView1.Columns[1].Width = (int)dataGridView1.Width / 3;
                dataGridView1.ReadOnly = false;
                dataGridView1.Columns[0].ReadOnly = true;

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
                }
            }
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            setup(false);
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            PaintImage(widthShow, heightShow, width, height);
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                draging = true;

        }
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {

            if (draging)
            {
                viewPortCenter = new PointF(viewPortCenter.X - ((lastMouse.X - e.X)), viewPortCenter.Y - ((lastMouse.Y - e.Y)));
                panel1.Invalidate();
            }
            x = (int)viewPortCenter.X;
            y = (int)viewPortCenter.Y;
            lastMouse = e.Location;
            setFileInfo();

        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
                draging = false;
        }

        private Image cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        private Image resizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

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

            return destImage;
        }

        private List<Image> createListImageCrop(List<Image> listImgOrg)
        {
            List<Image> listImgCrop = new List<Image>();

            foreach (Image img in listImgOrg)
            {
                Rectangle showRect = new Rectangle((int)(panel1.Width / 2.0f - widthShow / 2.0f), (int)(panel1.Height / 2.0f - heightShow / 2.0f), widthShow, heightShow);
                Rectangle drawRect = new Rectangle((int)(viewPortCenter.X - width / 2.0f), (int)(viewPortCenter.Y - height / 2), width, height);
                if (drawRect.IntersectsWith(drawRect))
                {
                    var intersectRect = Rectangle.Intersect(showRect, drawRect);
                    var cropRect = new Rectangle(intersectRect.X - drawRect.X, intersectRect.Y - drawRect.Y, intersectRect.Width, intersectRect.Height);

                    var cropImg = cropImage(resizeImage(img, width, height), cropRect);
                    var cellImg = resizeImage(cropImg, width / cellSize, height / cellSize);
                    listImgCrop.Add(cellImg);
                    //listImgCrop.Add(cropImg);
                    progressBarSave.PerformStep();
                }
            }
            return listImgCrop;
        }

        private List<Image> createListImage()
        {
            List<Image> listImage = new List<Image>();
            if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Cells[0].Value.ToString() != "")
            {
                string filename = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                int timeplay = 0;
                Int32.TryParse(dataGridView1.CurrentRow.Cells[1].Value.ToString(), out timeplay);
                var listImg = readFileTmp(filename);
                var coutImg = listImg.Count;
                if (coutImg > 0)
                {
                    var countLoop = timeplay * 1000 / interval;
                    for (int i = 0; i < countLoop; i++)
                    {
                        var j = i % (coutImg);
                        listImage.Add(listImg.ElementAt(j));
                    }
                }

            }
            //var cropImg = createListImageCrop(listImage);
            return listImage;
        }

        private void slider(Object source, EventArgs e)
        {
            if (index < ListImage.Count)
            {
                //re-draw
                bitmap = (Bitmap)ListImage[index];
                setup(false);
                index++;
            }
            else
            {
                T.Stop();
                isStart = false;
            }
        }



        private Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }


        private void setFileInfo()
        {
            this.tbX.Text = Convert.ToInt32(x).ToString();
            this.tbY.Text = Convert.ToInt32(y).ToString();
            this.tbWidth.Text = Convert.ToInt32(width / cellSize).ToString();
            this.tbHeight.Text = Convert.ToInt32(height / cellSize).ToString();

        }

        private void tbX_TextChanged(object sender, EventArgs e)
        {
            int temp;
            if (int.TryParse(tbX.Text, out temp))
            {
                x = temp;
                viewPortCenter.X = x;
                panel1.Invalidate();
            }
        }
        private void tbY_TextChanged(object sender, EventArgs e)
        {
            int temp;
            if (int.TryParse(tbY.Text, out temp))
            {
                y = temp;
                viewPortCenter.Y = y;
                panel1.Invalidate();
            }
        }
        private void tbWidth_TextChanged(object sender, EventArgs e)
        {
            int temp;
            if (int.TryParse(tbWidth.Text, out temp))
            {
                width = temp * cellSize;
                panel1.Invalidate();
            }
        }

        private void tbHeight_TextChanged(object sender, EventArgs e)
        {
            int temp;
            if (int.TryParse(tbHeight.Text, out temp))
            {
                height = temp * cellSize;
                panel1.Invalidate();
            }
        }

        private void btOpenFile_Click(object sender, EventArgs e)
        {
            openFile();
        }


        private void btSave_Click(object sender, EventArgs e)
        {
            progressBarSave.Visible = true;
            progressBarSave.Maximum = dataGridView1.Rows.Count;
            Main.CurrentArea.ListFileTemplates = new List<FileTemplate>();
            Main.CurrentArea.ListImages = new List<Image>();
            var timePlay = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value != null && !string.IsNullOrEmpty(row.Cells[0].Value.ToString()))
                {
                    string filename = row.Cells[0].Value.ToString();
                    int timeplay = 0;
                    Int32.TryParse(row.Cells[1].Value.ToString(), out timeplay);

                    var listImg = readFileTmp(filename);
                    if (timeplay > 0 && listImg.Count > 0)
                    {
                        FileTemplate file = new FileTemplate();
                        file.FileName = filename;
                        file.TimePlay = timeplay;
                        var listImgCrop = createListImageCrop(listImg);
                        file.ListImages.AddRange(listImgCrop);
                        Main.CurrentArea.ListFileTemplates.Add(file);
                        timePlay += timeplay;
                    }

                }
                progressBarSave.PerformLayout();
            }
            Main.CurrentArea.TimePlay = timePlay;
            MessageBox.Show(@"Save successfully!", @"Save");
            //Main.GetListImageOfArea(Main.CurrentArea);
            this.Dispose();
            this.Close();
        }



        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isStart = true;
            var listImage = createListImage();
            ListImage = new List<Image>();
            ListImage.AddRange(listImage);
            if (ListImage.Count > 0)
            {
                isStart = true;
                T.Start();
            }

        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isStart = false;
            T.Stop();
            //index = 0;
            //ListImage.Clear();
            //bitmap = (Bitmap)firstImage;
            isStart = false;
            setup(true);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Cells[0].Value.ToString() != "")
            {
                var filename = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                ListImage = new List<Image>();
                index = 0;
                T.Stop();
                var listImg = readFileTmp(filename);

                if (listImg.Count > 0)
                {
                    firstImage = listImg.ElementAt(0);
                    bitmap = (Bitmap)firstImage;
                    setup(false);
                }
            }
        }

    }
}
