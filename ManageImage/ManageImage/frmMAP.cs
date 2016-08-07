using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using ManageImage;

//using netDxf;


namespace LedFullControl
{
    public partial class frmMAP : Form
    {
        public frmMAP()
        {
            InitializeComponent();
            SetConfig();
            MakeBackgroundGrid();
        }

        void SetConfig()
        {
            KichThuoc.GridGap = 14;
            KichThuoc.CrossSize = 50;
            KTLamViec.ChieuRong = 200;
            KTLamViec.ChieuCao = 100;
        }

        void ReadConfig(int ChieuRong, int ChieuCao)
        {
            //	đọc dữ liệu config ở phần cấu hình
            //frmCauHinh DuLieu = new frmCauHinh();
            //KTLamViec.ChieuRong = ChieuRong;
            //KTLamViec.ChieuCao = ChieuCao;
            //MakeBackgroundGrid();
        }

        private void btnCauHinh_Click(object sender, EventArgs e)
        {
            //frmCauHinh ThietLap = new frmCauHinh();
            //ThietLap.Rong = KichThuoc.ChieuRong;
            //ThietLap.Cao = KichThuoc.ChieuCao;
            //ThietLap.TruyenConfig = new frmCauHinh.GetData(ReadConfig);		//	lấy dữ liệu từ form 2
            //ThietLap.ShowDialog();
        }

        private void frmMAP_Load(object sender, EventArgs e)
        {
            picDraw.BackColor = Color.Black;
        }

        //----------------------------------------------------------------------------------------------------------
        List<Point> FreeLED = new List<Point>();    //	led chưa được đi dây
                                                    //List<List<Point>> Line ;	//	Mảng led đã được đi dây
        List<Point> Line1 = new List<Point>();
        List<Point> Line2 = new List<Point>();
        //List<Point> Line3 = new List<Point>();
        List<Point> LedSelected = new List<Point>();    //	khi được chọn ta tạo ra mảng mới

        //		private int SoLine = new int();				//	Số line đã dùng
        private int[] SoLuongPhanTuList = new int[ListMax];     //	Số lượng phần tử trong mỗi mảng	
        const int ListMax = 100;
        int LineDangVe = new int();     //	line đang làm việc

        bool isDrawing;                 //	đang ở chế độ nối led

        private int CheDoVe;            //	Chế độ vẽ
        enum CheDo
        {
            Ranh,
            ThemLed,
            XoaLed,
            NoiLed,
            XoaVung,
            ChonVung,
            VeLed,
            VeVung,
            VeVungZ,
            VeDuongThang,
            DaoChieu
        }

        private Point FP, MP, LP = new Point(); //	Vị trí đầu , Vị trí sau
        private bool MP_OK = false; //	nếu vị trí 2 được kick

        private Point P2;   //	P2 : vị trí cuối cùng của chuột -> Mouse Move
        DinhNghia KichThuoc = new DinhNghia();
        DinhNghia KTLamViec = new DinhNghia();
        ToaDo TD = new ToaDo();

        #region Vẽ lưới cho vùng làm việc và SnapToGrid
        private void MakeBackgroundGrid()       //	Vẽ lưới nền
        {
            int ChieuRong, ChieuCao;
            ChieuRong = KichThuoc.GridGap * KTLamViec.ChieuRong;        //	Tính chiều rộng vùng làm việc
            ChieuCao = KichThuoc.GridGap * KTLamViec.ChieuCao;          //	Tính chiều cao vùng làm việc
            Bitmap bm = new Bitmap(ChieuRong, ChieuCao);                //	Tạo ra 1 cái ảnh có kích thước như trên
            for (int x = 0; x < ChieuRong; x += KichThuoc.GridGap)      //	không vẽ dấu khi x,y có tọa độ = 0
            {
                for (int y = 0; y < ChieuCao; y += KichThuoc.GridGap)
                {
                    if (x == 0 || y == 0) { bm.SetPixel(x, y, Color.Black); }
                    else { bm.SetPixel(x, y, Color.Blue); }
                }
            }
            picDraw.SizeMode = PictureBoxSizeMode.AutoSize;
            picDraw.Image = bm;
            picDraw.Refresh();
        }

        private void SnapToGrid(ref int x, ref int y)   //	Snap to the nearest grid point
        {
            x = KichThuoc.GridGap * (int)Math.Round((double)x / KichThuoc.GridGap);
            y = KichThuoc.GridGap * (int)Math.Round((double)y / KichThuoc.GridGap);
        }
        #endregion

        #region Phóng to - thu nhỏ		
        private void tbZoom_ValueChanged(object sender, EventArgs e)
        {
            rtxtThongTin.Text = " Phóng to : " + tbZoom.Value.ToString();   //	 Thông tin khi zoom
            KichThuoc.GridGap = tbZoom.Value;
            MakeBackgroundGrid();
        }
        #endregion

        /// <summary>
        /// Khoảng cách giữa  điểm
        /// </summary>
        /// <param name="pt1">Tọa độ điểm đầu</param>
        /// <param name="pt2">Tọa độ điểm cuối</param>
        /// <returns></returns>	
        private int KhoangCach(Point pt1, Point pt2)
        {
            int dx = pt1.X - pt2.X;
            int dy = pt1.Y - pt2.Y;
            return dx * dx + dy * dy;
        }

        private void picDraw_MouseMove(object sender, MouseEventArgs e)     //	sự kiện di chuyển chuột
        {
            int a = e.X;
            int b = e.Y;
            SnapToGrid(ref a, ref b);
            P2 = new Point(a, b);
            TD.TDNow = P2;
            Point ToaDoHienThi = new Point();
            ToaDoHienThi.X = P2.X / KichThuoc.GridGap;
            ToaDoHienThi.Y = P2.Y / KichThuoc.GridGap;
            //	-------------------------------------------------------
            txtToaDo.Text = (ToaDoHienThi.X.ToString() + " : " + ToaDoHienThi.Y.ToString());    //	hiển thị tọa độ đang vẽ
            txtTongLed.Text = (Line1.Count + Line2.Count).ToString();       //	Hiển thị số led tổng
            if (isDrawing)
            {
                if (Math.Abs(FP.X - P2.X / KichThuoc.GridGap) > 0 && Math.Abs(FP.Y - P2.Y / KichThuoc.GridGap) > 0)
                {
                    txtKichThuocVung.Text = string.Format(@"{0} : {1}", (Math.Abs(FP.X - P2.X / KichThuoc.GridGap) + 1).ToString(), (Math.Abs(FP.Y - P2.Y / KichThuoc.GridGap) + 1).ToString());
                }
            }
            else { txtKichThuocVung.Text = " -- : -- "; }
            #region Hiển thị trạng thái đang vẽ
            switch (CheDoVe)
            {
                case 0:
                    lbCheDoVe.Text = "Rảnh"; break;
                case 1:
                    lbCheDoVe.Text = "Thêm LED"; break;
                case 2:
                    lbCheDoVe.Text = "Xóa LED"; break;
                case 3:
                    lbCheDoVe.Text = "Nối LED"; break;
                case 4:
                    lbCheDoVe.Text = "Xóa Vùng"; break;
                case 5:
                    lbCheDoVe.Text = "Chọn Vùng"; break;
            }


            #endregion
            #region Hiển thị trạng thái lên bảng thông tin
            //			rtxtThongTin.Text = "Line đang vẽ : " + LineDangVe.ToString() +"\n";

            #endregion
            picDraw.Invalidate();
        }
        /// <summary>
        /// Vẽ 1 bóng led
        /// </summary>
        /// <param name="TD">Tọa độ vẽ</param>
        void VeLed(Point TD)    //	tọa độ của điểm / GridGap
        {
            Graphics Ve = picDraw.CreateGraphics();
            Brush FillLed = new SolidBrush(Color.Blue);
            Ve.FillEllipse(FillLed, TD.X * KichThuoc.GridGap - KichThuoc.BanKinhLed, TD.Y * KichThuoc.GridGap - KichThuoc.BanKinhLed, KichThuoc.LedSize, KichThuoc.LedSize);
            Ve.Dispose();
            FillLed.Dispose();
            //			Pen VienLed = new Pen(Color.Yellow, 1);
            //			Ve.DrawEllipse(VienLed, TD.X * KichThuoc.GridGap - KichThuoc.BanKinhLed, TD.Y * KichThuoc.GridGap - KichThuoc.BanKinhLed, KichThuoc.LedSize, KichThuoc.LedSize);
        }

        #region Toàn bộ phần hiển thị trên màn hình PicDraw nằm ở đây
        private void picDraw_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawLine(Pens.YellowGreen, 0, P2.Y, panelPicDraw.Width, P2.Y);
            e.Graphics.DrawLine(Pens.YellowGreen, P2.X, 0, P2.X, panelPicDraw.Height);
            //e.Graphics.DrawLine(Pens.YellowGreen, P2.X - KichThuoc.CrossSize, P2.Y, P2.X + KichThuoc.CrossSize, P2.Y);
            //e.Graphics.DrawLine(Pens.YellowGreen, P2.X, P2.Y - KichThuoc.CrossSize, P2.X, P2.Y + KichThuoc.CrossSize);
            rtxtThongTin.Text = string.Format("Rộng hiển thị  {0} : {1}", panelPicDraw.Width, panelPicDraw.Height);
            //---------------------------------------
            Pen GrenPen = new Pen(Color.Green, 2);
            Brush FillLed = new SolidBrush(Color.Blue);

            foreach (Point pt in FreeLED)   //	Vẽ led free 
            {
                Rectangle rect = new Rectangle(pt.X * KichThuoc.GridGap - KichThuoc.BanKinhLed, pt.Y * KichThuoc.GridGap - KichThuoc.BanKinhLed,
                                                KichThuoc.LedSize, KichThuoc.LedSize);
                e.Graphics.FillEllipse(Brushes.White, rect);
            }
            #region Vẽ led line1,2 và đường nối led
            Pen P = new Pen(Color.Red, 2);
            for (int i = 0; i < Line1.Count - 1; i++)
            {
                Point pt1, pt2 = new Point();
                pt1 = Line1[i];
                pt2 = Line1[i + 1];
                pt1.X *= KichThuoc.GridGap;
                pt1.Y *= KichThuoc.GridGap;
                pt2.X *= KichThuoc.GridGap;
                pt2.Y *= KichThuoc.GridGap;

                e.Graphics.DrawLine(P, pt1, pt2);
            }
            foreach (Point pt in Line1)
            {
                Rectangle rect = new Rectangle(pt.X * KichThuoc.GridGap - KichThuoc.BanKinhLed, pt.Y * KichThuoc.GridGap - KichThuoc.BanKinhLed,
                                                KichThuoc.LedSize, KichThuoc.LedSize);
                e.Graphics.FillEllipse(Brushes.Blue, rect);
            }
            if (Line1.Count > 0)        //	Vẽ led đầu màu đỏ
            {
                Rectangle Frist = new Rectangle(Line1[0].X * KichThuoc.GridGap - KichThuoc.BanKinhLed, Line1[0].Y * KichThuoc.GridGap - KichThuoc.BanKinhLed,
                                    KichThuoc.LedSize, KichThuoc.LedSize);
                e.Graphics.FillEllipse(Brushes.Green, Frist);
            }
            //-----------------------------------------------------
            //	Vẽ led line2 và đường nối led Line 2
            //			Pen P = new Pen(Color.Red,2);
            for (int i = 0; i < Line2.Count - 1; i++)
            {
                Point pt1, pt2 = new Point();
                pt1 = Line2[i];
                pt2 = Line2[i + 1];
                pt1.X *= KichThuoc.GridGap;
                pt1.Y *= KichThuoc.GridGap;
                pt2.X *= KichThuoc.GridGap;
                pt2.Y *= KichThuoc.GridGap;

                e.Graphics.DrawLine(P, pt1, pt2);
            }
            foreach (Point pt in Line2)
            {
                Rectangle rect = new Rectangle(pt.X * KichThuoc.GridGap - KichThuoc.BanKinhLed, pt.Y * KichThuoc.GridGap - KichThuoc.BanKinhLed,
                                                KichThuoc.LedSize, KichThuoc.LedSize);
                e.Graphics.FillEllipse(Brushes.Blue, rect);
            }
            if (Line2.Count > 0)        //	Vẽ led đầu màu đỏ
            {
                Rectangle Frist = new Rectangle(Line2[0].X * KichThuoc.GridGap - KichThuoc.BanKinhLed, Line2[0].Y * KichThuoc.GridGap - KichThuoc.BanKinhLed,
                                    KichThuoc.LedSize, KichThuoc.LedSize);
                e.Graphics.FillEllipse(Brushes.Red, Frist);
            }

            #endregion
            //------------------------------------------------
            #region Vẽ đường kẻ đứt khi nối led
            Point DiemCuoiLine = new Point();
            if (CheDoVe == (int)CheDo.NoiLed || CheDoVe == (int)CheDo.VeLed)
            {
                Pen NetDut = new Pen(Color.White);
                switch (LineDangVe)
                {
                    case 1:
                        if (Line1.Count > 0)
                        {
                            DiemCuoiLine = Line1[Line1.Count - 1];
                            DiemCuoiLine.X *= KichThuoc.GridGap;
                            DiemCuoiLine.Y *= KichThuoc.GridGap;
                            NetDut.DashPattern = new float[] { 3f, 3f };
                            e.Graphics.DrawLine(NetDut, DiemCuoiLine, P2);
                        }
                        break;
                    case 2:
                        if (Line2.Count > 0)
                        {
                            DiemCuoiLine = Line2[Line2.Count - 1];
                            DiemCuoiLine.X *= KichThuoc.GridGap;
                            DiemCuoiLine.Y *= KichThuoc.GridGap;
                            NetDut.DashPattern = new float[] { 3f, 3f };
                            e.Graphics.DrawLine(NetDut, DiemCuoiLine, P2);
                        }
                        break;
                }
            }
            #endregion
            //------------------------------------------------
            #region Vẽ hình chữ nhật biểu thị đang chọn vùng
            else if (CheDoVe == (int)CheDo.ChonVung || CheDoVe == (int)CheDo.XoaVung)
            {
                if (isDrawing)
                {
                    //					Rectangle ChonVung = new Rectangle(FP.X * KichThuoc.GridGap, FP.Y * KichThuoc.GridGap, P2.X, P2.Y);
                    e.Graphics.DrawLine(GrenPen, FP.X * KichThuoc.GridGap, FP.Y * KichThuoc.GridGap, FP.X * KichThuoc.GridGap, P2.Y);   //	X
                    e.Graphics.DrawLine(GrenPen, FP.X * KichThuoc.GridGap, FP.Y * KichThuoc.GridGap, P2.X, FP.Y * KichThuoc.GridGap);
                    e.Graphics.DrawLine(GrenPen, P2.X, FP.Y * KichThuoc.GridGap, P2.X, P2.Y);
                    e.Graphics.DrawLine(GrenPen, FP.X * KichThuoc.GridGap, P2.Y, P2.X, P2.Y);
                }
            }
            #endregion
            //------------------------------------------------
            #region Vẽ line biểu thị khi chọn vẽ vùng
            else if (CheDoVe == (int)CheDo.VeVung)
            {
                if (isDrawing)
                {
                    if (!MP_OK)
                    {
                        int CX, CY, KQ;     //	kiểm tra xem vẽ theo chiều nào
                        CX = P2.X - FP.X * KichThuoc.GridGap;
                        CY = P2.Y - FP.Y * KichThuoc.GridGap;
                        KQ = Math.Abs(CX) - Math.Abs(CY);
                        if (KQ > 0) //	Theo chiều X
                        {
                            e.Graphics.DrawLine(Pens.Cyan, FP.X * KichThuoc.GridGap, FP.Y * KichThuoc.GridGap, P2.X, FP.Y * KichThuoc.GridGap);
                        }
                        else
                        {
                            e.Graphics.DrawLine(Pens.Cyan, FP.X * KichThuoc.GridGap, FP.Y * KichThuoc.GridGap, FP.X * KichThuoc.GridGap, P2.Y);
                        }
                    }
                    else if (MP_OK)
                    {
                        bool Tren = false;
                        #region Theo chiều X
                        if (MP.Y == FP.Y)       //	Theo chiều X
                        {
                            if (P2.Y / KichThuoc.GridGap > MP.Y)
                            {
                                for (int i = 0; i <= P2.Y / KichThuoc.GridGap - MP.Y; i++)
                                {
                                    e.Graphics.DrawLine(Pens.Cyan, (FP.X) * KichThuoc.GridGap, (FP.Y + i) * KichThuoc.GridGap,
                                                                    (MP.X) * KichThuoc.GridGap, (MP.Y + i) * KichThuoc.GridGap);
                                }
                                for (int i = 0; i < P2.Y / KichThuoc.GridGap - MP.Y; i++)
                                {
                                    Tren = !Tren;
                                    if (Tren)
                                    {
                                        e.Graphics.DrawLine(Pens.Cyan, (MP.X) * KichThuoc.GridGap, (MP.Y + i) * KichThuoc.GridGap,
                                                                        (MP.X) * KichThuoc.GridGap, (MP.Y + i + 1) * KichThuoc.GridGap);
                                    }
                                    else
                                    {
                                        e.Graphics.DrawLine(Pens.Cyan, (FP.X) * KichThuoc.GridGap, (FP.Y + i) * KichThuoc.GridGap,
                                                                        (FP.X) * KichThuoc.GridGap, (FP.Y + i + 1) * KichThuoc.GridGap);
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i <= MP.Y - P2.Y / KichThuoc.GridGap; i++)
                                {
                                    e.Graphics.DrawLine(Pens.Cyan, (FP.X) * KichThuoc.GridGap, (FP.Y - i) * KichThuoc.GridGap,
                                                                    (MP.X) * KichThuoc.GridGap, (MP.Y - i) * KichThuoc.GridGap);
                                }
                                for (int i = 0; i < MP.Y - P2.Y / KichThuoc.GridGap; i++)
                                {
                                    Tren = !Tren;
                                    if (Tren)
                                    {
                                        e.Graphics.DrawLine(Pens.Cyan, (MP.X) * KichThuoc.GridGap, (MP.Y - i) * KichThuoc.GridGap,
                                                                        (MP.X) * KichThuoc.GridGap, (MP.Y - i - 1) * KichThuoc.GridGap);
                                    }
                                    else
                                    {
                                        e.Graphics.DrawLine(Pens.Cyan, (FP.X) * KichThuoc.GridGap, (FP.Y - i) * KichThuoc.GridGap,
                                                                        (FP.X) * KichThuoc.GridGap, (FP.Y - i - 1) * KichThuoc.GridGap);
                                    }
                                }
                            }
                        }
                        #endregion
                        #region Theo chiều Y
                        else if (MP.X == FP.X)  //	Theo chiều Y
                        {
                            if (P2.X / KichThuoc.GridGap > MP.X)
                            {
                                for (int i = 0; i <= P2.X / KichThuoc.GridGap - MP.X; i++)
                                {
                                    e.Graphics.DrawLine(Pens.Cyan, (FP.X + i) * KichThuoc.GridGap, (FP.Y) * KichThuoc.GridGap,
                                                                    (MP.X + i) * KichThuoc.GridGap, (MP.Y) * KichThuoc.GridGap);
                                }
                                Tren = true;
                                for (int i = 0; i < P2.X / KichThuoc.GridGap - MP.X; i++)
                                {
                                    Tren = !Tren;
                                    if (Tren)
                                    {
                                        e.Graphics.DrawLine(Pens.Cyan, (FP.X + i) * KichThuoc.GridGap, (FP.Y) * KichThuoc.GridGap,
                                                                        (FP.X + i + 1) * KichThuoc.GridGap, (FP.Y) * KichThuoc.GridGap);
                                    }
                                    else
                                    {
                                        e.Graphics.DrawLine(Pens.Cyan, (MP.X + i) * KichThuoc.GridGap, (MP.Y) * KichThuoc.GridGap,
                                                                        (MP.X + i + 1) * KichThuoc.GridGap, (MP.Y) * KichThuoc.GridGap);
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i <= MP.X - P2.X / KichThuoc.GridGap; i++)
                                {
                                    e.Graphics.DrawLine(Pens.Cyan, (FP.X - i) * KichThuoc.GridGap, (FP.Y) * KichThuoc.GridGap,
                                                                    (MP.X - i) * KichThuoc.GridGap, (MP.Y) * KichThuoc.GridGap);
                                }
                                Tren = true;
                                for (int i = 0; i < MP.X - P2.X / KichThuoc.GridGap; i++)
                                {
                                    Tren = !Tren;
                                    if (Tren)
                                    {
                                        e.Graphics.DrawLine(Pens.Cyan, (FP.X - i) * KichThuoc.GridGap, (FP.Y) * KichThuoc.GridGap,
                                                                        (FP.X - i - 1) * KichThuoc.GridGap, (FP.Y) * KichThuoc.GridGap);
                                    }
                                    else
                                    {
                                        e.Graphics.DrawLine(Pens.Cyan, (MP.X - i) * KichThuoc.GridGap, (MP.Y) * KichThuoc.GridGap,
                                                                        (MP.X - i - 1) * KichThuoc.GridGap, (MP.Y) * KichThuoc.GridGap);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            #endregion
            //------------------------------------------------
            #region Vẽ Vùng Z
            else if (CheDoVe == (int)CheDo.VeVungZ)
            {
                if (isDrawing)
                {
                    if (!MP_OK)
                    {
                        int CX, CY, KQ;     //	kiểm tra xem vẽ theo chiều nào
                        CX = P2.X - FP.X * KichThuoc.GridGap;
                        CY = P2.Y - FP.Y * KichThuoc.GridGap;
                        KQ = Math.Abs(CX) - Math.Abs(CY);
                        if (KQ > 0) //	Theo chiều X
                        {
                            e.Graphics.DrawLine(Pens.Cyan, FP.X * KichThuoc.GridGap, FP.Y * KichThuoc.GridGap, P2.X, FP.Y * KichThuoc.GridGap);
                        }
                        else
                        {
                            e.Graphics.DrawLine(Pens.Cyan, FP.X * KichThuoc.GridGap, FP.Y * KichThuoc.GridGap, FP.X * KichThuoc.GridGap, P2.Y);
                        }
                    }
                    else if (MP_OK)
                    {
                        //						bool Tren = false;
                        #region Theo chiều X
                        if (MP.Y == FP.Y)       //	Theo chiều X
                        {
                            if (P2.Y / KichThuoc.GridGap > MP.Y)
                            {
                                for (int i = 0; i <= P2.Y / KichThuoc.GridGap - MP.Y; i++)
                                {
                                    e.Graphics.DrawLine(Pens.Cyan, (FP.X) * KichThuoc.GridGap, (FP.Y + i) * KichThuoc.GridGap,
                                                                    (MP.X) * KichThuoc.GridGap, (MP.Y + i) * KichThuoc.GridGap);
                                }
                                //for (int i = 0; i < P2.Y / KichThuoc.GridGap - MP.Y; i++)
                                //{
                                //	Tren = !Tren;
                                //	if (Tren)
                                //	{
                                //		e.Graphics.DrawLine(Pens.Cyan, (MP.X) * KichThuoc.GridGap, (MP.Y+i) * KichThuoc.GridGap,
                                //										(MP.X) * KichThuoc.GridGap, (MP.Y+i+1) * KichThuoc.GridGap);
                                //	}
                                //	else
                                //	{
                                //		e.Graphics.DrawLine(Pens.Cyan, (FP.X) * KichThuoc.GridGap, (FP.Y+i) * KichThuoc.GridGap,
                                //										(FP.X) * KichThuoc.GridGap, (FP.Y+i+1) * KichThuoc.GridGap);
                                //	}
                                //}
                            }
                            else
                            {
                                for (int i = 0; i <= MP.Y - P2.Y / KichThuoc.GridGap; i++)
                                {
                                    e.Graphics.DrawLine(Pens.Cyan, (FP.X) * KichThuoc.GridGap, (FP.Y - i) * KichThuoc.GridGap,
                                                                    (MP.X) * KichThuoc.GridGap, (MP.Y - i) * KichThuoc.GridGap);
                                }
                                //for (int i = 0; i < MP.Y - P2.Y / KichThuoc.GridGap; i++)
                                //{
                                //	Tren = !Tren;
                                //	if (Tren)
                                //	{
                                //		e.Graphics.DrawLine(Pens.Cyan, (MP.X) * KichThuoc.GridGap, (MP.Y-i) * KichThuoc.GridGap,
                                //										(MP.X) * KichThuoc.GridGap, (MP.Y-i-1) * KichThuoc.GridGap);
                                //	}
                                //	else
                                //	{
                                //		e.Graphics.DrawLine(Pens.Cyan, (FP.X) * KichThuoc.GridGap, (FP.Y-i) * KichThuoc.GridGap,
                                //										(FP.X) * KichThuoc.GridGap, (FP.Y-i-1) * KichThuoc.GridGap);
                                //	}
                                //}								
                            }
                        }
                        #endregion
                        #region Theo chiều Y
                        else if (MP.X == FP.X)  //	Theo chiều Y
                        {
                            if (P2.X / KichThuoc.GridGap > MP.X)
                            {
                                for (int i = 0; i <= P2.X / KichThuoc.GridGap - MP.X; i++)
                                {
                                    e.Graphics.DrawLine(Pens.Cyan, (FP.X + i) * KichThuoc.GridGap, (FP.Y) * KichThuoc.GridGap,
                                                                    (MP.X + i) * KichThuoc.GridGap, (MP.Y) * KichThuoc.GridGap);
                                }
                                //Tren = true;
                                //for (int i = 0; i < P2.X / KichThuoc.GridGap - MP.X; i++)
                                //{
                                //	Tren = !Tren;
                                //	if (Tren)
                                //	{
                                //		e.Graphics.DrawLine(Pens.Cyan, (FP.X+i) * KichThuoc.GridGap, (FP.Y) * KichThuoc.GridGap,
                                //										(FP.X+i+1) * KichThuoc.GridGap, (FP.Y) * KichThuoc.GridGap);
                                //	}
                                //	else
                                //	{
                                //		e.Graphics.DrawLine(Pens.Cyan, (MP.X+i) * KichThuoc.GridGap, (MP.Y) * KichThuoc.GridGap,
                                //										(MP.X+i+1) * KichThuoc.GridGap, (MP.Y) * KichThuoc.GridGap);
                                //	}
                                //}
                            }
                            else
                            {
                                for (int i = 0; i <= MP.X - P2.X / KichThuoc.GridGap; i++)
                                {
                                    e.Graphics.DrawLine(Pens.Cyan, (FP.X - i) * KichThuoc.GridGap, (FP.Y) * KichThuoc.GridGap,
                                                                    (MP.X - i) * KichThuoc.GridGap, (MP.Y) * KichThuoc.GridGap);
                                }
                                //Tren = true;
                                //for (int i = 0; i < MP.X - P2.X / KichThuoc.GridGap; i++)
                                //{
                                //	Tren = !Tren;
                                //	if (Tren)
                                //	{
                                //		e.Graphics.DrawLine(Pens.Cyan, (FP.X-i) * KichThuoc.GridGap, (FP.Y) * KichThuoc.GridGap,
                                //										(FP.X-i-1) * KichThuoc.GridGap, (FP.Y) * KichThuoc.GridGap);
                                //	}
                                //	else
                                //	{
                                //		e.Graphics.DrawLine(Pens.Cyan, (MP.X-i) * KichThuoc.GridGap, (MP.Y) * KichThuoc.GridGap,
                                //										(MP.X-i-1) * KichThuoc.GridGap, (MP.Y) * KichThuoc.GridGap);
                                //	}
                                //}
                            }
                        }
                        #endregion
                    }
                }
            }
            #endregion
            FillLed.Dispose();
            GrenPen.Dispose();
        }
        #endregion
        /// <summary>
        /// Kiểm tra xem điểm led thuộc line nào
        /// </summary>
        /// <param name="pt">Điểm cần kiểm tra</param>
        /// <returns>Kết quả. nếu -1 thì không ở line nào</returns>
        private int CheckLedInLine(Point pt)
        {
            //for (int _Line = 0; _Line < SoLine; _Line++)
            //{
            //	for (int i = 0; i < Line[_Line].Count; i++)
            //	{
            //		if (pt == Line[_Line][i])
            //		{
            //			return _Line;	
            //		}
            //	}
            //}
            return -1;
        }
        #region Kiểm tra xem led ở line nào ?		
        private bool LedInLine1(Point pt)
        {
            foreach (Point item in Line1)
            {
                if (item == pt)
                {
                    return true;
                }
            }
            return false;
        }
        private bool LedInLine2(Point pt)
        {
            foreach (Point item in Line2)
            {
                if (item == pt)
                {
                    return true;
                }
            }
            return false;
        }
        private bool LedInLine3(Point pt)
        {
            //foreach (Point  item in Line3)
            //{
            //	if (item == pt )
            //	{
            //		return true;
            //	}
            //}
            return false;
        }
        #endregion
        #region Kiểm tra vị trí của led nằm trong mảng

        private int LedInDexLine1(Point pt)
        {
            for (int i = 0; i < Line1.Count; i++)
            {
                if (Line1[i] == pt)
                {
                    return i;
                }
            }
            return -1;
        }
        private int LedInDexLine2(Point pt)
        {
            for (int i = 0; i < Line2.Count; i++)
            {
                if (Line2[i] == pt)
                {
                    return i;
                }
            }
            return -1;
        }
        private int LedInDexLine3(Point pt)
        {
            for (int i = 0; i < Line1.Count; i++)
            {
                if (Line1[i] == pt)
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion
        //	Sự kiện kick chuột
        private void picDraw_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)     //	Nếu là kick chuột phải thì thoát các chế độ
            {
                CheDoVe = (int)CheDo.Ranh;
                isDrawing = false;
                MP_OK = false;
            }
            else
            {   //	Tính vị trí điểm
                Point NewP1 = new Point();
                float x, y;     //	Lấy giá trị nhảy lưới	
                x = e.Location.X % KichThuoc.GridGap;
                y = e.Location.Y % KichThuoc.GridGap;
                if (x > KichThuoc.GridGap / 2) { NewP1.X = e.Location.X / KichThuoc.GridGap + 1; }  //	căn lại tọa độ trỏ chuột.
                else { NewP1.X = e.Location.X / KichThuoc.GridGap; }                                //	khi ở giữa 2 điểm grid

                if (y > KichThuoc.GridGap / 2) { NewP1.Y = e.Location.Y / KichThuoc.GridGap + 1; }  //	nó sẽ chọn grid gần nhất để vẽ
                else { NewP1.Y = e.Location.Y / KichThuoc.GridGap; }

                TD.VuaKick = NewP1;

                if (NewP1.X > 0 && NewP1.Y > 0)     //	thêm led ở chỗ này
                {
                    switch (CheDoVe)
                    {
                        #region Rảnh						
                        case (int)CheDo.Ranh:
                            //	Chọn line để vẽ
                            if (LedInLine1(NewP1)) { LineDangVe = 1; }
                            else if (LedInLine2(NewP1)) { LineDangVe = 2; }
                            break;
                        #endregion
                        #region Thêm FreeLED
                        case (int)CheDo.ThemLed:    //	Thêm led đơn
                            if (!LedInFreeLED(NewP1))
                            {
                                FreeLED.Add(NewP1);
                            }
                            break;
                        #endregion
                        #region Xóa LED						
                        case (int)CheDo.XoaLed:
                            if (LedInFreeLED(NewP1))
                            {
                                FreeLED.Remove(NewP1);
                            }
                            if (LedInLine1(NewP1))
                            {
                                Line1.Remove(NewP1);
                            }
                            if (LedInLine2(NewP1))
                            {
                                Line2.Remove(NewP1);
                            }
                            break;
                        #endregion
                        #region Nối led
                        case (int)CheDo.NoiLed:
                            if (isDrawing)  //	nếu là chế độ vẽ 
                            {
                                switch (LineDangVe)
                                {
                                    case 1:
                                        if (LedInFreeLED(NewP1))
                                        {
                                            Line1.Add(NewP1);
                                            FreeLED.Remove(NewP1);
                                        }
                                        else if (LedInLine2(NewP1))     //	chuyển toàn bộ led sang line 1
                                        {
                                            int VT = LedInDexLine2(NewP1);
                                            int DaiLine2 = Line2.Count;
                                            for (int i = VT; i < DaiLine2; i++)
                                            {
                                                Line1.Add(Line2[i]);
                                            }

                                            for (int i = VT; i < DaiLine2; i++) //	xóa 
                                            {
                                                Line2.Remove(Line2[VT]);
                                            }
                                        }
                                        else if (LedInLine1(NewP1))
                                        {

                                        }
                                        else
                                        {
                                            Line1.Add(NewP1);
                                        }
                                        break;

                                    case 2:
                                        if (LedInFreeLED(NewP1))
                                        {
                                            Line2.Add(NewP1);
                                            FreeLED.Remove(NewP1);
                                        }
                                        else if (LedInLine1(NewP1))     //	chuyển toàn bộ led sang line 2
                                        {
                                            int VT = LedInDexLine1(NewP1);
                                            int DaiLine1 = Line1.Count;
                                            for (int i = VT; i < DaiLine1; i++)
                                            {
                                                Line2.Add(Line1[i]);
                                            }
                                            for (int i = VT; i < DaiLine1; i++) //	xóa 
                                            {
                                                Line1.Remove(Line1[VT]);
                                            }
                                        }
                                        else if (LedInLine2(NewP1))
                                        {
                                            //	do nothing
                                        }
                                        else
                                        {
                                            Line2.Add(NewP1);
                                        }
                                        break;
                                }
                            }
                            else    //	Not Drawing
                            {
                                if (LedInFreeLED(NewP1))    //	kiểm tra xem led vừa kick ở line nào
                                {
                                    if (Line1.Count == 0)
                                    {
                                        Line1.Add(NewP1);
                                        LineDangVe = 1;
                                    }
                                    else if (Line2.Count == 0)
                                    {
                                        Line2.Add(NewP1);
                                        LineDangVe = 2;
                                    }
                                    FreeLED.Remove(NewP1);
                                }
                                else if (LedInLine1(NewP1))
                                {
                                    LineDangVe = 1;

                                }
                                else if (LedInLine2(NewP1))
                                {
                                    LineDangVe = 2;
                                }
                                else
                                {
                                    Line1.Add(NewP1);
                                    LineDangVe = 1;
                                }
                                isDrawing = true;
                            }
                            break;
                        #endregion
                        #region Vẽ Led						
                        case (int)CheDo.VeLed:
                            if (isDrawing)  //	nếu là chế độ vẽ 
                            {
                                FP = NewP1;
                                TD.VuaKick = NewP1;
                                switch (LineDangVe)
                                {
                                    case 1:
                                        if (LedInFreeLED(NewP1))
                                        {
                                            Line1.Add(NewP1);
                                            FreeLED.Remove(NewP1);
                                        }
                                        else if (!LedInLine2(NewP1) || !LedInLine1(NewP1))
                                        {
                                            #region Vẽ nhanh khi ấn phím Ctrl + Kick
                                            if (Control.ModifierKeys == Keys.Control)   //	vẽ nhanh với phím Control
                                            {
                                                Point Moi = new Point();

                                                if (Line1[Line1.Count - 1].Y == NewP1.Y)    //	Nếu là chiều ngang
                                                {
                                                    if (Line1[Line1.Count - 1].X > NewP1.X) //	Lùi
                                                    {
                                                        int X2 = Line1[Line1.Count - 1].X;
                                                        for (int i = 1; i <= X2 - NewP1.X; i++)
                                                        {
                                                            Moi.X = X2 - i;
                                                            Moi.Y = NewP1.Y;
                                                            if (!LedInLine1(Moi) || !LedInLine1(Moi))
                                                            {
                                                                Line1.Add(Moi);
                                                            }
                                                        }
                                                    }
                                                    else if (Line1[Line1.Count - 1].X < NewP1.X)    //	Tiến
                                                    {
                                                        int X2 = Line1[Line1.Count - 1].X;
                                                        for (int i = 1; i <= NewP1.X - X2; i++)
                                                        {
                                                            Moi.X = X2 + i;
                                                            Moi.Y = NewP1.Y;
                                                            if (!LedInLine1(Moi) || !LedInLine1(Moi))
                                                            {
                                                                Line1.Add(Moi);
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (Line1[Line1.Count - 1].X == NewP1.X)   //	Nếu là chiều Dọc
                                                {
                                                    if (Line1[Line1.Count - 1].Y > NewP1.Y) //	Lùi
                                                    {
                                                        int Y2 = Line1[Line1.Count - 1].Y;
                                                        for (int i = 1; i <= Y2 - NewP1.Y; i++)
                                                        {
                                                            Moi.Y = Y2 - i;
                                                            Moi.X = NewP1.X;
                                                            if (!LedInLine1(Moi) || !LedInLine1(Moi))
                                                            {
                                                                Line1.Add(Moi);
                                                            }
                                                        }
                                                    }
                                                    else if (Line1[Line1.Count - 1].Y < NewP1.Y)    //	Tiến
                                                    {
                                                        int Y2 = Line1[Line1.Count - 1].Y;
                                                        for (int i = 1; i <= NewP1.Y - Y2; i++)
                                                        {
                                                            Moi.Y = Y2 + i;
                                                            Moi.X = NewP1.X;
                                                            if (LedInLine1(Moi) || LedInLine1(Moi))
                                                            {
                                                                //	Không làm gì cả
                                                            }
                                                            else
                                                            {
                                                                Line1.Add(Moi);
                                                            }
                                                        }
                                                    }
                                                }
                                                else    //	Vẽ led theo đường chéo
                                                {
                                                    if (true)
                                                    {

                                                    }
                                                }
                                            }
                                            #endregion
                                            else
                                            {
                                                Line1.Add(NewP1);
                                            }
                                        }
                                        break;

                                    case 2:
                                        if (LedInFreeLED(NewP1))
                                        {
                                            Line2.Add(NewP1);
                                            FreeLED.Remove(NewP1);
                                        }
                                        else if (!LedInLine1(NewP1) || !LedInLine2(NewP1))
                                        {
                                            #region Vẽ nhanh khi ấn phím Ctrl + Kick											
                                            if (Control.ModifierKeys == Keys.Control)   //	vẽ nhanh với phím Control
                                            {
                                                Point Moi = new Point();

                                                if (Line2[Line2.Count - 1].Y == NewP1.Y)    //	Nếu là chiều ngang
                                                {
                                                    if (Line2[Line2.Count - 1].X > NewP1.X) //	Lùi
                                                    {
                                                        int X2 = Line2[Line2.Count - 1].X;
                                                        for (int i = 1; i <= X2 - NewP1.X; i++)
                                                        {
                                                            Moi.X = X2 - i;
                                                            Moi.Y = NewP1.Y;
                                                            if (!LedInLine1(Moi) || !LedInLine2(Moi))
                                                            {
                                                                Line2.Add(Moi);
                                                            }
                                                        }
                                                    }
                                                    else if (Line2[Line2.Count - 1].X < NewP1.X)    //	Tiến
                                                    {
                                                        int X2 = Line2[Line2.Count - 1].X;
                                                        for (int i = 1; i <= NewP1.X - X2; i++)
                                                        {
                                                            Moi.X = X2 + i;
                                                            Moi.Y = NewP1.Y;
                                                            if (!LedInLine1(Moi) || !LedInLine2(Moi))
                                                            {
                                                                Line2.Add(Moi);
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (Line2[Line2.Count - 1].X == NewP1.X)   //	Nếu là chiều Dọc
                                                {
                                                    if (Line2[Line2.Count - 1].Y > NewP1.Y) //	Lùi
                                                    {
                                                        int Y2 = Line2[Line2.Count - 1].Y;
                                                        for (int i = 1; i <= Y2 - NewP1.Y; i++)
                                                        {
                                                            Moi.Y = Y2 - i;
                                                            Moi.X = NewP1.X;
                                                            if (!LedInLine1(Moi) || !LedInLine2(Moi))
                                                            {
                                                                Line2.Add(Moi);
                                                            }
                                                        }
                                                    }
                                                    else if (Line2[Line2.Count - 1].Y < NewP1.Y)    //	Tiến
                                                    {
                                                        int Y2 = Line2[Line2.Count - 1].Y;
                                                        for (int i = 1; i <= NewP1.Y - Y2; i++)
                                                        {
                                                            Moi.Y = Y2 + i;
                                                            Moi.X = NewP1.X;
                                                            if (LedInLine1(Moi) || LedInLine2(Moi))
                                                            {
                                                                //	Không làm gì cả
                                                            }
                                                            else
                                                            {
                                                                Line2.Add(Moi);
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (Math.Abs(TD.TDNow.X - TD.VuaKick.X) == Math.Abs(TD.TDNow.Y - TD.VuaKick.Y))    //	Vẽ led theo đường chéo
                                                {

                                                }
                                            }
                                            #endregion
                                            else
                                            {
                                                Line2.Add(NewP1);
                                            }
                                        }
                                        break;
                                }
                            }
                            else    //	Not Drawing
                            {
                                if (LedInFreeLED(NewP1))    //	kiểm tra xem led vừa kick ở line nào
                                {
                                    if (Line1.Count == 0)
                                    {
                                        Line1.Add(NewP1);
                                        LineDangVe = 1;
                                    }
                                    else if (Line2.Count == 0)
                                    {
                                        Line2.Add(NewP1);
                                        LineDangVe = 2;
                                    }
                                    FreeLED.Remove(NewP1);
                                }
                                else if (LedInLine1(NewP1))
                                {
                                    LineDangVe = 1;

                                }
                                else if (LedInLine2(NewP1))
                                {
                                    LineDangVe = 2;
                                }
                                else
                                {
                                    if (Line2.Count > Line1.Count)
                                    {
                                        Line1.Add(NewP1);
                                        LineDangVe = 1;
                                    }
                                    else
                                    {
                                        Line2.Add(NewP1);
                                        LineDangVe = 2;
                                    }
                                }
                                isDrawing = true;
                            }
                            break;
                        #endregion
                        #region Xóa vùng
                        case (int)CheDo.XoaVung:
                            if (!isDrawing)
                            {
                                FP = NewP1;     //	Lấy tọa độ điểm đầu
                                isDrawing = true;
                                rtxtThongTin.Text = "FP : " + FP.X.ToString() + " | " + FP.Y.ToString();
                            }
                            else
                            {
                                LP = NewP1;     //	Lấy tọa độ điểm sau
                                isDrawing = false;
                                //	Lọc tất cả các led có tọa độ nằm trong khoảng đã chọn . điểm nào trùng thì xóa
                                if (LP.X > FP.X && LP.Y > FP.Y)
                                {
                                    for (int xx = FP.X; xx < LP.X; xx++)
                                    {
                                        for (int yy = FP.Y; yy < LP.Y; yy++)
                                        {
                                            Point DiemCheck = new Point(xx, yy);
                                            if (LedInLine1(DiemCheck))
                                            {
                                                Line1.Remove(DiemCheck);
                                            }
                                            else if (LedInLine2(DiemCheck))
                                            {
                                                Line2.Remove(DiemCheck);
                                            }
                                        }
                                    }
                                }
                                else if (LP.X < FP.X && LP.Y > FP.Y)
                                {
                                    for (int xx = LP.X; xx < FP.X; xx++)
                                    {
                                        for (int yy = FP.Y; yy < LP.Y; yy++)
                                        {
                                            Point DiemCheck = new Point(xx, yy);
                                            if (LedInLine1(DiemCheck))
                                            {
                                                Line1.Remove(DiemCheck);
                                            }
                                            else if (LedInLine2(DiemCheck))
                                            {
                                                Line2.Remove(DiemCheck);
                                            }
                                        }
                                    }
                                }
                                else if (LP.X > FP.X && LP.Y < FP.Y)
                                {
                                    for (int xx = FP.X; xx < LP.X; xx++)
                                    {
                                        for (int yy = LP.Y; yy < FP.Y; yy++)
                                        {
                                            Point DiemCheck = new Point(xx, yy);
                                            if (LedInLine1(DiemCheck))
                                            {
                                                Line1.Remove(DiemCheck);
                                            }
                                            else if (LedInLine2(DiemCheck))
                                            {
                                                Line2.Remove(DiemCheck);
                                            }
                                        }
                                    }
                                }
                                else if (LP.X < FP.X && LP.Y < FP.Y)
                                {
                                    for (int xx = LP.X; xx < FP.X; xx++)
                                    {
                                        for (int yy = LP.Y; yy < FP.Y; yy++)
                                        {
                                            Point DiemCheck = new Point(xx, yy);
                                            if (LedInLine1(DiemCheck))
                                            {
                                                Line1.Remove(DiemCheck);
                                            }
                                            else if (LedInLine2(DiemCheck))
                                            {
                                                Line2.Remove(DiemCheck);
                                            }
                                        }
                                    }
                                }
                            }

                            break;
                        #endregion
                        #region Vẽ Vùng
                        case (int)CheDo.VeVung:
                            if (!isDrawing)     //	kick lần 1
                            {
                                FP = NewP1;
                                isDrawing = true;
                            }
                            else if (isDrawing && !MP_OK)   //	Kick lần 2
                            {
                                int CX, CY, KQ;
                                MP_OK = true;
                                MP = NewP1;
                                CX = MP.X - FP.X;
                                CY = MP.Y - FP.Y;
                                KQ = Math.Abs(CX) - Math.Abs(CY);
                                if (KQ > 0) //	Chiều y dai hơn chiều x. Lấy chiều Y
                                {
                                    MP.Y = FP.Y;
                                }
                                else        //	lấy chiều X
                                {
                                    MP.X = FP.X;
                                }
                            }
                            else if (isDrawing && MP_OK)    //	kick lần 3
                            {                           //		
                                MP_OK = false;          //		
                                isDrawing = false;
                                LP = NewP1; //	Tọa độ điểm cuối
                                bool Xuoi = false;
                                if (MP.X == FP.X)
                                {
                                    //	thêm mảng led vào đây
                                    #region Đi chiều X, Kéo xuống - đi sang phải
                                    if (LP.X > FP.X)
                                    {
                                        if (FP.Y < MP.Y)    //	Kéo xuống
                                        {
                                            for (int xx = 0; xx <= LP.X - FP.X; xx++)
                                            {
                                                Xuoi = !Xuoi;
                                                if (Xuoi)
                                                {
                                                    for (int yy = 0; yy <= MP.Y - FP.Y; yy++)
                                                    {
                                                        Point Moi = new Point(FP.X + xx, FP.Y + yy);
                                                        if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                        {
                                                            Line1.Add(Moi);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    for (int yy = MP.Y - FP.Y; yy >= 0; yy--)
                                                    {
                                                        Point Moi = new Point(FP.X + xx, FP.Y + yy);
                                                        if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                        {
                                                            Line1.Add(Moi);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (FP.Y > MP.Y)   //	Kéo lên 
                                        {
                                            for (int xx = 0; xx <= LP.X - FP.X; xx++)
                                            {
                                                Xuoi = !Xuoi;
                                                if (Xuoi)
                                                {
                                                    for (int yy = 0; yy <= FP.Y - MP.Y; yy++)
                                                    {
                                                        Point Moi = new Point(FP.X + xx, FP.Y - yy);
                                                        Line1.Add(Moi);
                                                    }
                                                }
                                                else
                                                {
                                                    for (int yy = FP.Y - MP.Y; yy >= 0; yy--)
                                                    {
                                                        Point Moi = new Point(FP.X + xx, FP.Y - yy);
                                                        Line1.Add(Moi);
                                                    }
                                                }
                                            }
                                            //--------------------------------------------
                                        }
                                    }
                                    #endregion
                                    #region Đi chiều X, Kéo xuống - đi sang trái
                                    if (LP.X < FP.X)        //	đi sang trái
                                    {
                                        if (FP.Y < MP.Y)    //	Kéo xuống
                                        {
                                            for (int xx = 0; xx <= FP.X - LP.X; xx++)       //	lấy số điểm ngang - X
                                            {
                                                Xuoi = !Xuoi;
                                                if (Xuoi)
                                                {
                                                    for (int yy = 0; yy <= MP.Y - FP.Y; yy++)
                                                    {
                                                        Point Moi = new Point(FP.X - xx, FP.Y + yy);
                                                        if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                        {
                                                            Line1.Add(Moi);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    for (int yy = MP.Y - FP.Y; yy >= 0; yy--)
                                                    {
                                                        Point Moi = new Point(FP.X - xx, FP.Y + yy);
                                                        if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                        {
                                                            Line1.Add(Moi);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (FP.Y > MP.Y)   //	Kéo lên 
                                        {
                                            for (int xx = 0; xx <= FP.X - LP.X; xx++)
                                            {
                                                Xuoi = !Xuoi;
                                                if (Xuoi)
                                                {
                                                    for (int yy = 0; yy <= FP.Y - MP.Y; yy++)
                                                    {
                                                        Point Moi = new Point(FP.X - xx, FP.Y - yy);
                                                        Line1.Add(Moi);
                                                    }
                                                }
                                                else
                                                {
                                                    for (int yy = FP.Y - MP.Y; yy >= 0; yy--)
                                                    {
                                                        Point Moi = new Point(FP.X - xx, FP.Y - yy);
                                                        Line1.Add(Moi);
                                                    }
                                                }
                                            }
                                            //--------------------------------------------
                                        }
                                    }
                                    #endregion
                                }
                                else if (FP.Y == MP.Y)
                                {
                                    //	thêm mảng led vào đây
                                    #region Đi theo chiều Y , đi xuống
                                    if (LP.Y > FP.Y)
                                    {
                                        if (FP.X < MP.X)    //	Kéo sang phải
                                        {
                                            for (int yy = 0; yy <= LP.Y - FP.Y; yy++)
                                            {
                                                Xuoi = !Xuoi;
                                                if (Xuoi)
                                                {
                                                    for (int xx = 0; xx <= MP.X - FP.X; xx++)
                                                    {
                                                        Point Moi = new Point(FP.X + xx, FP.Y + yy);
                                                        if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                        {
                                                            Line1.Add(Moi);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    for (int xx = MP.X - FP.X; xx >= 0; xx--)
                                                    {
                                                        Point Moi = new Point(FP.X + xx, FP.Y + yy);
                                                        if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                        {
                                                            Line1.Add(Moi);
                                                        }
                                                    }
                                                }
                                            }//	for
                                        }
                                        else    //	Kéo sang trái
                                        {
                                            for (int yy = 0; yy <= LP.Y - FP.Y; yy++)
                                            {
                                                Xuoi = !Xuoi;
                                                if (Xuoi)
                                                {
                                                    for (int xx = 0; xx <= FP.X - MP.X; xx++)
                                                    {
                                                        Point Moi = new Point(FP.X - xx, FP.Y + yy);
                                                        if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                        {
                                                            Line1.Add(Moi);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    for (int xx = FP.X - MP.X; xx >= 0; xx--)
                                                    {
                                                        Point Moi = new Point(FP.X - xx, FP.Y + yy);
                                                        if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                        {
                                                            Line1.Add(Moi);
                                                        }
                                                    }
                                                }
                                            }
                                        }//else									
                                    }
                                    #endregion
                                    #region Đi theo chiều Y đi lên
                                    else if (LP.Y < FP.Y)       //	Kéo lên trên
                                    {
                                        if (FP.X < MP.X)    //	Kéo sang phải
                                        {
                                            for (int yy = 0; yy <= FP.Y - LP.Y; yy++)
                                            {
                                                Xuoi = !Xuoi;
                                                if (Xuoi)
                                                {
                                                    for (int xx = 0; xx <= MP.X - FP.X; xx++)
                                                    {
                                                        Point Moi = new Point(FP.X + xx, FP.Y - yy);
                                                        if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                        {
                                                            Line1.Add(Moi);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    for (int xx = MP.X - FP.X; xx >= 0; xx--)
                                                    {
                                                        Point Moi = new Point(FP.X + xx, FP.Y - yy);
                                                        if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                        {
                                                            Line1.Add(Moi);
                                                        }
                                                    }
                                                }
                                            }//	for
                                        }//(FP.X < MP.X)
                                        if (FP.X > MP.X)    //	Kéo sang phải
                                        {
                                            for (int yy = 0; yy <= FP.Y - LP.Y; yy++)
                                            {
                                                Xuoi = !Xuoi;
                                                if (Xuoi)
                                                {
                                                    for (int xx = 0; xx <= FP.X - MP.X; xx++)
                                                    {
                                                        Point Moi = new Point(FP.X - xx, FP.Y - yy);
                                                        if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                        {
                                                            Line1.Add(Moi);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    for (int xx = FP.X - MP.X; xx >= 0; xx--)
                                                    {
                                                        Point Moi = new Point(FP.X - xx, FP.Y - yy);
                                                        if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                        {
                                                            Line1.Add(Moi);
                                                        }
                                                    }
                                                }
                                            }//	for
                                        }//(FP.X < MP.X)
                                    }
                                    #endregion
                                }
                            }
                            break;
                        #endregion
                        #region Vẽ Vùng Z
                        case (int)CheDo.VeVungZ:
                            if (!isDrawing)     //	kick lần 1
                            {
                                FP = NewP1;
                                isDrawing = true;
                            }
                            else if (isDrawing && !MP_OK)   //	Kick lần 2
                            {
                                int CX, CY, KQ;
                                MP_OK = true;
                                MP = NewP1;
                                CX = MP.X - FP.X;
                                CY = MP.Y - FP.Y;
                                KQ = Math.Abs(CX) - Math.Abs(CY);
                                if (KQ > 0) //	Chiều y dai hơn chiều x. Lấy chiều Y
                                {
                                    MP.Y = FP.Y;
                                }
                                else        //	lấy chiều X
                                {
                                    MP.X = FP.X;
                                }
                            }
                            else if (isDrawing && MP_OK)    //	kick lần 3
                            {                           //		
                                MP_OK = false;          //		
                                isDrawing = false;
                                LP = NewP1; //	Tọa độ điểm cuối
                                if (MP.X == FP.X)
                                {
                                    //	thêm mảng led vào đây
                                    #region Đi chiều X, Kéo xuống - đi sang phải
                                    if (LP.X > FP.X)
                                    {
                                        if (FP.Y < MP.Y)    //	Kéo xuống
                                        {
                                            for (int xx = 0; xx <= LP.X - FP.X; xx++)
                                            {
                                                for (int yy = 0; yy <= MP.Y - FP.Y; yy++)
                                                {
                                                    Point Moi = new Point(FP.X + xx, FP.Y + yy);
                                                    if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                    {
                                                        Line1.Add(Moi);
                                                    }
                                                }
                                            }
                                        }
                                        else if (FP.Y > MP.Y)   //	Kéo lên 
                                        {
                                            for (int xx = 0; xx <= LP.X - FP.X; xx++)
                                            {
                                                for (int yy = 0; yy <= FP.Y - MP.Y; yy++)
                                                {
                                                    Point Moi = new Point(FP.X + xx, FP.Y - yy);
                                                    Line1.Add(Moi);
                                                }
                                            }
                                            //--------------------------------------------
                                        }
                                    }
                                    #endregion
                                    #region Đi chiều X, Kéo xuống - đi sang trái
                                    if (LP.X < FP.X)        //	đi sang trái
                                    {
                                        if (FP.Y < MP.Y)    //	Kéo xuống
                                        {
                                            for (int xx = 0; xx <= FP.X - LP.X; xx++)       //	lấy số điểm ngang - X
                                            {
                                                for (int yy = 0; yy <= MP.Y - FP.Y; yy++)
                                                {
                                                    Point Moi = new Point(FP.X - xx, FP.Y + yy);
                                                    if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                    {
                                                        Line1.Add(Moi);
                                                    }
                                                }
                                            }
                                        }
                                        else if (FP.Y > MP.Y)   //	Kéo lên 
                                        {
                                            for (int xx = 0; xx <= FP.X - LP.X; xx++)
                                            {
                                                for (int yy = 0; yy <= FP.Y - MP.Y; yy++)
                                                {
                                                    Point Moi = new Point(FP.X - xx, FP.Y - yy);
                                                    Line1.Add(Moi);
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else if (FP.Y == MP.Y)
                                {
                                    //	thêm mảng led vào đây
                                    #region Đi theo chiều Y , đi xuống
                                    if (LP.Y > FP.Y)
                                    {
                                        if (FP.X < MP.X)    //	Kéo sang phải
                                        {
                                            for (int yy = 0; yy <= LP.Y - FP.Y; yy++)
                                            {
                                                for (int xx = 0; xx <= MP.X - FP.X; xx++)
                                                {
                                                    Point Moi = new Point(FP.X + xx, FP.Y + yy);
                                                    if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                    {
                                                        Line1.Add(Moi);
                                                    }
                                                }
                                            }//	for
                                        }
                                        else    //	Kéo sang trái
                                        {
                                            for (int yy = 0; yy <= LP.Y - FP.Y; yy++)
                                            {
                                                for (int xx = 0; xx <= FP.X - MP.X; xx++)
                                                {
                                                    Point Moi = new Point(FP.X - xx, FP.Y + yy);
                                                    if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                    {
                                                        Line1.Add(Moi);
                                                    }
                                                }
                                            }
                                        }//else									
                                    }
                                    #endregion
                                    #region Đi theo chiều Y đi lên
                                    else if (LP.Y < FP.Y)       //	Kéo lên trên
                                    {
                                        if (FP.X < MP.X)    //	Kéo sang phải
                                        {
                                            for (int yy = 0; yy <= FP.Y - LP.Y; yy++)
                                            {
                                                for (int xx = 0; xx <= MP.X - FP.X; xx++)
                                                {
                                                    Point Moi = new Point(FP.X + xx, FP.Y - yy);
                                                    if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                    {
                                                        Line1.Add(Moi);
                                                    }
                                                }
                                            }//	for
                                        }//(FP.X < MP.X)
                                        if (FP.X > MP.X)    //	Kéo sang phải
                                        {
                                            for (int yy = 0; yy <= FP.Y - LP.Y; yy++)
                                            {
                                                for (int xx = 0; xx <= FP.X - MP.X; xx++)
                                                {
                                                    Point Moi = new Point(FP.X - xx, FP.Y - yy);
                                                    if (!LedInLine1(Moi) && !LedInLine2(Moi))   //	bỏ qua vị trí có led
                                                    {
                                                        Line1.Add(Moi);
                                                    }
                                                }
                                            }//	for
                                        }//(FP.X < MP.X)
                                    }
                                    #endregion
                                }
                            }
                            break;
                            #endregion
                    }
                }
                //	cái này để debug
            }
        }

        /// <summary>
        /// kiểm tra xem led vừa được thêm vào có nằm trong list đã có hay không ( mảng FreeLed )
        /// </summary>
        /// <param name="pt">Tọa độ kiểm tra</param>
        /// <returns>Kết quả</returns>
        private bool LedInFreeLED(Point pt) //	đè led. Led mới đè led cũ ( cái này chỉ tính ở mảng FreeLed )
        {
            foreach (Point item in FreeLED)
            {
                if (item == pt)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra xem led có nằm trong các line(list) không
        /// </summary>
        /// <param name="pt">Tọa độ kiểm tra</param>
        /// <returns>Kết quả</returns>
        private bool LedInLine(Point pt)
        {
            //for (int _DaySo = 0; _DaySo < SoLine; _DaySo++)
            //{
            //	for (int _LedSo = 0; _LedSo < Line[_DaySo].Count; _LedSo++)
            //	{
            //		if (pt == Line[_DaySo][_LedSo])
            //		{
            //			return true;
            //		}
            //	}
            //} 
            return false;
        }

        #region Xử lý các nút ấn ở đây
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnVe_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.NoiLed;
        }
        private void btnThemLed_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.ThemLed;
        }
        private void btnXoaLed_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.XoaLed;
        }
        private void btnNoiLed_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.NoiLed;
        }
        private void btnVeLed_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.VeLed;
        }
        private void btnXoaVung_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.XoaVung;
        }
        private void btnVeVung_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.VeVung;
        }
        private void btnVeVungZ_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.VeVungZ;
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            FreeLED.Clear();
            Line1.Clear();
            Line2.Clear();
            picDraw.Invalidate();
        }
        protected bool SaveDataToFile(string filename, byte[] Data, int length)
        {
            BinaryWriter writer = null;
            try
            {
                writer = new BinaryWriter(File.OpenWrite(filename));
                writer.Write(Data, 0, length);
                writer.Flush();
                writer.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            string TenFile;
            saveFile.Filter = "SmartFull Map File (.map) | *.map";
            if (saveFile.ShowDialog() == DialogResult.OK)       //	
            {
                TenFile = saveFile.FileName;
                int DataSize = Line1.Count * 4 + Line2.Count * 4 + FreeLED.Count * 4 + 20;  //	1 point có 4 byte ( 2byte x, 2byte y )
                byte[] Data = new byte[DataSize];

                #region Lấy thuộc tính vùng làm việc		
                Data[0] = (byte)(KTLamViec.ChieuRong / 256);
                Data[1] = (byte)(KTLamViec.ChieuRong % 256);
                Data[2] = (byte)(KTLamViec.ChieuCao / 256);
                Data[3] = (byte)(KTLamViec.ChieuCao % 256);
                Data[4] = (byte)(Line1.Count / 256);
                Data[5] = (byte)(Line1.Count % 256);
                Data[6] = (byte)(Line2.Count / 256);
                Data[7] = (byte)(Line2.Count % 256);
                Data[8] = (byte)(FreeLED.Count / 256);
                Data[9] = (byte)(FreeLED.Count % 256);  //	FreeLed
                #endregion
                //-------------------------------------------------------------
                #region Line 1			
                int Dai1 = Line1.Count;
                byte[] buf1 = new byte[Dai1 * 4];

                for (int i = 0; i < Dai1; i++)
                {
                    int H = Line1[i].X / 256;
                    int L = Line1[i].X % 256;
                    buf1[i * 4] = (byte)H;
                    buf1[i * 4 + 1] = (byte)L;

                    H = Line1[i].Y / 256;
                    L = Line1[i].Y % 256;
                    buf1[i * 4 + 2] = (byte)H;
                    buf1[i * 4 + 3] = (byte)L;
                }
                for (int i = 0; i < Dai1 * 4; i++)
                {
                    Data[i + 20] = buf1[i];
                }
                #endregion
                //--------------------------------------------------------------
                #region Line 2				
                int Dai2 = Line2.Count;
                byte[] buf2 = new byte[Dai2 * 4];

                for (int i = 0; i < Dai2; i++)
                {
                    int H = Line2[i].X / 256;
                    int L = Line2[i].X % 256;
                    buf2[i * 4] = (byte)H;
                    buf2[i * 4 + 1] = (byte)L;

                    H = Line2[i].Y / 256;
                    L = Line2[i].Y % 256;
                    buf2[i * 4 + 2] = (byte)H;
                    buf2[i * 4 + 3] = (byte)L;
                }
                for (int i = 0; i < Dai2 * 4; i++)
                {
                    Data[i + 20 + Dai1 * 4] = buf2[i];
                }
                #endregion
                //--------------------------------------------------------------
                #region FreeLED		
                int DaiFree = FreeLED.Count;
                byte[] buf0 = new byte[DaiFree * 4];

                for (int i = 0; i < DaiFree; i++)
                {
                    int H = FreeLED[i].X / 256;
                    int L = FreeLED[i].X % 256;
                    buf0[i * 4] = (byte)H;
                    buf0[i * 4 + 1] = (byte)L;

                    H = FreeLED[i].Y / 256;
                    L = FreeLED[i].Y % 256;
                    buf0[i * 4 + 2] = (byte)H;
                    buf0[i * 4 + 3] = (byte)L;
                }
                for (int i = 0; i < DaiFree * 4; i++)
                {
                    Data[i + 20 + Dai1 * 4 + Dai2 * 4] = buf0[i];
                }
                #endregion

                SaveDataToFile(TenFile, Data, DataSize);    //	ghi dữ liệu ra file
            }
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            ofdFile.Filter = "File cấu hình đi dây (*.map)|*.map|All files (*.*)|*.*";
            if (ofdFile.ShowDialog() == DialogResult.OK)
            {
                FileStream F = new FileStream(ofdFile.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                byte[] Data = new byte[F.Length];
                F.Position = 0; //	Vị trí bắt đầu đọc dữ liệu
                for (int i = 0; i < F.Length; i++)
                {
                    Data[i] = (byte)F.ReadByte();
                }
                F.Close();  //	Đóng file khi đọc xong

                KTLamViec.ChieuRong = Data[0] * 256 + Data[1];      //	kích thước làm việc
                KTLamViec.ChieuCao = Data[2] * 256 + Data[3];
                MakeBackgroundGrid();   //	Vẽ lại kích thước vùng làm việc

                //	Lấy dữ liệu tọa độ cho các line 
                Line1.Clear();
                Line2.Clear();
                FreeLED.Clear();    //	làm trống các line
                int[] Dai = new int[3]; //	Chiều dài các line
                Dai[0] = Data[8] * 256 + Data[9];
                Dai[1] = Data[4] * 256 + Data[5];
                Dai[2] = Data[6] * 256 + Data[7];
                //	lấy dữ liệu Line 1
                Point P = new Point();
                for (int i = 0; i < Dai[1]; i++)
                {
                    P.X = Data[i * 4 + 20] * 256 + Data[i * 4 + 21];
                    P.Y = Data[i * 4 + 22] * 256 + Data[i * 4 + 23];
                    Line1.Add(P);
                }
                for (int i = 0; i < Dai[2]; i++)
                {
                    P.X = Data[i * 4 + 20 + Dai[1] * 4] * 256 + Data[i * 4 + 21 + Dai[1] * 4];
                    P.Y = Data[i * 4 + 22 + Dai[1] * 4] * 256 + Data[i * 4 + 23 + Dai[1] * 4];
                    Line2.Add(P);
                }

                for (int i = 0; i < Dai[0]; i++)
                {
                    P.X = Data[i * 4 + 20 + Dai[1] * 4 + Dai[2] * 4] * 256 + Data[i * 4 + 21 + Dai[1] * 4 + Dai[2] * 4];
                    P.Y = Data[i * 4 + 22 + Dai[1] * 4 + Dai[2] * 4] * 256 + Data[i * 4 + 23 + Dai[1] * 4 + Dai[2] * 4];
                    FreeLED.Add(P);
                }
            }
        }
        #endregion

        #region Nút thoát form và lưu lại data
        private void frmMAP_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                int DataSize = Line1.Count * 4 + Line2.Count * 4 + FreeLED.Count * 4 + 20;  //	1 point có 4 byte ( 2byte x, 2byte y )
                byte[] Data = new byte[DataSize];
                //cal width/height:
                if (Line1.Count > 0)
                {
                    Program.main.maxX = Line1.Max(m => m.X);
                    Program.main.maxY = Line1.Max(m => m.Y);
                    Program.main.minX = Line1.Min(m => m.X);
                    Program.main.minY = Line1.Min(m => m.Y);
                    Program.main.line1 = Line1;
                    Program.main.line2 = Line2;
                    Program.main.ReSizePanel();
                }
                else
                {
                    Program.main.ClearMap();
                }
                #region Lấy thuộc tính vùng làm việc
                Data[0] = (byte)(KTLamViec.ChieuRong / 256);
                Data[1] = (byte)(KTLamViec.ChieuRong % 256);
                Data[2] = (byte)(KTLamViec.ChieuCao / 256);
                Data[3] = (byte)(KTLamViec.ChieuCao % 256);
                Data[4] = (byte)(Line1.Count / 256);
                Data[5] = (byte)(Line1.Count % 256);
                Data[6] = (byte)(Line2.Count / 256);
                Data[7] = (byte)(Line2.Count % 256);
                Data[8] = (byte)(FreeLED.Count / 256);
                Data[9] = (byte)(FreeLED.Count % 256);  //	FreeLed
                #endregion
                //-------------------------------------------------------------
                #region Line 1
                int Dai1 = Line1.Count;
                byte[] buf1 = new byte[Dai1 * 4];

                for (int i = 0; i < Dai1; i++)
                {
                    int H = Line1[i].X / 256;
                    int L = Line1[i].X % 256;
                    buf1[i * 4] = (byte)H;
                    buf1[i * 4 + 1] = (byte)L;

                    H = Line1[i].Y / 256;
                    L = Line1[i].Y % 256;
                    buf1[i * 4 + 2] = (byte)H;
                    buf1[i * 4 + 3] = (byte)L;
                }
                for (int i = 0; i < Dai1 * 4; i++)
                {
                    Data[i + 20] = buf1[i];
                }
                #endregion
                //--------------------------------------------------------------
                #region Line 2
                int Dai2 = Line2.Count;
                byte[] buf2 = new byte[Dai2 * 4];

                for (int i = 0; i < Dai2; i++)
                {
                    int H = Line2[i].X / 256;
                    int L = Line2[i].X % 256;
                    buf2[i * 4] = (byte)H;
                    buf2[i * 4 + 1] = (byte)L;

                    H = Line2[i].Y / 256;
                    L = Line2[i].Y % 256;
                    buf2[i * 4 + 2] = (byte)H;
                    buf2[i * 4 + 3] = (byte)L;
                }
                for (int i = 0; i < Dai2 * 4; i++)
                {
                    Data[i + 20 + Dai1 * 4] = buf2[i];
                }
                #endregion
                //--------------------------------------------------------------
                #region FreeLED
                int DaiFree = FreeLED.Count;
                byte[] buf0 = new byte[DaiFree * 4];

                for (int i = 0; i < DaiFree; i++)
                {
                    int H = FreeLED[i].X / 256;
                    int L = FreeLED[i].X % 256;
                    buf0[i * 4] = (byte)H;
                    buf0[i * 4 + 1] = (byte)L;

                    H = FreeLED[i].Y / 256;
                    L = FreeLED[i].Y % 256;
                    buf0[i * 4 + 2] = (byte)H;
                    buf0[i * 4 + 3] = (byte)L;
                }
                for (int i = 0; i < DaiFree * 4; i++)
                {
                    Data[i + 20 + Dai1 * 4 + Dai2 * 4] = buf0[i];
                }
                #endregion

                SaveDataToFile(@"SmartFull Data.lon", Data, DataSize);  //	ghi dữ liệu ra file
            }
            catch
            {
                MessageBox.Show(@"Éo kết chuyển data được", @"Có lỗi không rõ");
                throw;
            }
            //StreamWriter str = File.AppendText(@"c:\Xin loi.doc");
            //str.WriteLine("ghi gi tuy thich");
            //str.Close();
        }
        #endregion

        #region Trình xử lý phím tắt với ToolStripMenu
        private void btnDaoChieu_Click(object sender, EventArgs e)
        {
            List<Point> Buffer = new List<Point>();
            int SoLed = Line1.Count;
            switch (LineDangVe)
            {
                case 1:
                    rtxtThongTin.Text = "Đảo chiều Line 1";
                    for (int i = 0; i < SoLed; i++) //	copy toàn bộ mảng led sang 1 mảng mới
                    {
                        Buffer.Add(Line1[i]);   //	copy
                    }
                    Line1.Clear();      //	Xóa List

                    for (int i = SoLed; i > 0; i--) //	copy lại
                    {
                        Line1.Add(Buffer[i - 1]);
                    }
                    picDraw.Invalidate();           //	vẽ lại
                    break;

                case 2:
                    rtxtThongTin.Text = "Đảo chiều Line 2";
                    for (int i = 0; i < SoLed; i++) //	copy toàn bộ mảng led sang 1 mảng mới
                    {
                        Buffer.Add(Line1[i]);       //	copy
                    }
                    Line2.Clear();          //	Xóa List

                    for (int i = SoLed; i > 0; i--) //	copy lại
                    {
                        Line2.Add(Buffer[i - 1]);
                    }
                    picDraw.Invalidate();   //	vẽ lại
                    break;
            }
        }
        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnOpen_Click(sender, e);
        }
        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnSave_Click(sender, e);
        }
        private void addFreeLedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.ThemLed;
        }
        private void connectLedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.NoiLed;
        }
        private void drawLedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.VeLed;
        }
        private void drawAreaLedZicZacToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.VeVung;
        }
        private void drawAreaLedChữZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.VeVungZ;
        }
        private void removeLedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.XoaLed;
        }
        private void removeAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheDoVe = (int)CheDo.XoaVung;
        }
        private void RemoveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void InvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnDaoChieu_Click(sender, e);
        }
        private void ZoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (KichThuoc.GridGap < 20)
            {
                KichThuoc.GridGap += 2;
                tbZoom.Value = KichThuoc.GridGap;
                rtxtThongTin.Text = " Phóng to : " + tbZoom.Value.ToString();   //	 Thông tin khi zoom
                MakeBackgroundGrid();
            }
        }
        private void ZoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (KichThuoc.GridGap > 4)
            {
                KichThuoc.GridGap -= 2;
                tbZoom.Value = KichThuoc.GridGap;
                rtxtThongTin.Text = " Thu nhỏ : " + tbZoom.Value.ToString();    //	 Thông tin khi zoom
                MakeBackgroundGrid();
            }
        }
        private void SettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnCauHinh_Click(sender, e);
        }
        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Thanks for use SmartFull V3", "upgrading ...");
        }
        private void NewFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult TraLoi;
            TraLoi = MessageBox.Show("Tùy chọn sẽ xóa hết dữ liệu file.  \nBạn có muốn lưu lại công việc không ?", "Thông báo", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (TraLoi == DialogResult.Yes)
            {
                btnSave_Click(sender, e);
            }
            else if (TraLoi == DialogResult.No)
            {
                btnClear_Click(sender, e);
            }
        }
        private void importDXFFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "File DXF (*.dxf)|*.dxf|All files (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string TenFile = ofd.FileName;

            }
        }
        #endregion

        private void picDraw_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Hide();
        }

        private void picDraw_MouseLeave(object sender, EventArgs e)
        {
            Cursor.Show();
        }

    }
}







