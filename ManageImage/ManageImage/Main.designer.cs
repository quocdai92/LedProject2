namespace ManageImage
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new ManageImage.FormEdit.overRidePanel();
            this.dgvListArea = new System.Windows.Forms.DataGridView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableEditionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnNewArea = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbkAutoScroll = new System.Windows.Forms.CheckBox();
            this.lblMapTotalLedValue = new System.Windows.Forms.Label();
            this.lblMapTotalLed = new System.Windows.Forms.Label();
            this.lblMapHeightValue = new System.Windows.Forms.Label();
            this.lblMapHeight = new System.Windows.Forms.Label();
            this.lblMapWidthValue = new System.Windows.Forms.Label();
            this.lblMapWidth = new System.Windows.Forms.Label();
            this.trkbGridSize = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnMap = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnEnableEdit = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvListArea)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkbGridSize)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(423, 526);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseUp);
            // 
            // dgvListArea
            // 
            this.dgvListArea.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvListArea.Location = new System.Drawing.Point(12, 148);
            this.dgvListArea.Name = "dgvListArea";
            this.dgvListArea.ReadOnly = true;
            this.dgvListArea.Size = new System.Drawing.Size(294, 227);
            this.dgvListArea.TabIndex = 1;
            this.dgvListArea.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvListArea_CellDoubleClick);
            this.dgvListArea.SelectionChanged += new System.EventHandler(this.dgvListArea_SelectionChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(988, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem,
            this.saveFileToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // saveFileToolStripMenuItem
            // 
            this.saveFileToolStripMenuItem.Name = "saveFileToolStripMenuItem";
            this.saveFileToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.saveFileToolStripMenuItem.Text = "SaveFile";
            this.saveFileToolStripMenuItem.Visible = false;
            this.saveFileToolStripMenuItem.Click += new System.EventHandler(this.saveFileToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableEditionToolStripMenuItem,
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.optionsToolStripMenuItem.Text = "Công Cụ";
            // 
            // enableEditionToolStripMenuItem
            // 
            this.enableEditionToolStripMenuItem.Enabled = false;
            this.enableEditionToolStripMenuItem.Name = "enableEditionToolStripMenuItem";
            this.enableEditionToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.enableEditionToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.enableEditionToolStripMenuItem.Text = "Cho Phép Chỉnh Sửa";
            this.enableEditionToolStripMenuItem.Click += new System.EventHandler(this.enableEditionToolStripMenuItem_Click);
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.startToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.stopToolStripMenuItem.Text = "Stop";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
            // 
            // btnNewArea
            // 
            this.btnNewArea.Location = new System.Drawing.Point(12, 46);
            this.btnNewArea.Name = "btnNewArea";
            this.btnNewArea.Size = new System.Drawing.Size(39, 23);
            this.btnNewArea.TabIndex = 3;
            this.btnNewArea.Text = "New";
            this.btnNewArea.UseVisualStyleBackColor = true;
            this.btnNewArea.Click += new System.EventHandler(this.btnNewArea_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(110, 46);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(47, 23);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbkAutoScroll);
            this.groupBox1.Controls.Add(this.lblMapTotalLedValue);
            this.groupBox1.Controls.Add(this.lblMapTotalLed);
            this.groupBox1.Controls.Add(this.lblMapHeightValue);
            this.groupBox1.Controls.Add(this.lblMapHeight);
            this.groupBox1.Controls.Add(this.lblMapWidthValue);
            this.groupBox1.Controls.Add(this.lblMapWidth);
            this.groupBox1.Controls.Add(this.trkbGridSize);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 393);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(282, 165);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Thông Số Màn Hình";
            // 
            // cbkAutoScroll
            // 
            this.cbkAutoScroll.AutoSize = true;
            this.cbkAutoScroll.Location = new System.Drawing.Point(147, 134);
            this.cbkAutoScroll.Name = "cbkAutoScroll";
            this.cbkAutoScroll.Size = new System.Drawing.Size(96, 17);
            this.cbkAutoScroll.TabIndex = 10;
            this.cbkAutoScroll.Text = "Tự Động Cuộn";
            this.cbkAutoScroll.UseVisualStyleBackColor = true;
            // 
            // lblMapTotalLedValue
            // 
            this.lblMapTotalLedValue.AutoSize = true;
            this.lblMapTotalLedValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMapTotalLedValue.Location = new System.Drawing.Point(239, 90);
            this.lblMapTotalLedValue.Name = "lblMapTotalLedValue";
            this.lblMapTotalLedValue.Size = new System.Drawing.Size(0, 17);
            this.lblMapTotalLedValue.TabIndex = 8;
            // 
            // lblMapTotalLed
            // 
            this.lblMapTotalLed.AutoSize = true;
            this.lblMapTotalLed.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMapTotalLed.Location = new System.Drawing.Point(144, 90);
            this.lblMapTotalLed.Name = "lblMapTotalLed";
            this.lblMapTotalLed.Size = new System.Drawing.Size(94, 17);
            this.lblMapTotalLed.TabIndex = 7;
            this.lblMapTotalLed.Text = "Tổng Số Led:";
            // 
            // lblMapHeightValue
            // 
            this.lblMapHeightValue.AutoSize = true;
            this.lblMapHeightValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMapHeightValue.Location = new System.Drawing.Point(53, 133);
            this.lblMapHeightValue.Name = "lblMapHeightValue";
            this.lblMapHeightValue.Size = new System.Drawing.Size(0, 17);
            this.lblMapHeightValue.TabIndex = 6;
            // 
            // lblMapHeight
            // 
            this.lblMapHeight.AutoSize = true;
            this.lblMapHeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMapHeight.Location = new System.Drawing.Point(6, 133);
            this.lblMapHeight.Name = "lblMapHeight";
            this.lblMapHeight.Size = new System.Drawing.Size(37, 17);
            this.lblMapHeight.TabIndex = 5;
            this.lblMapHeight.Text = "Cao:";
            // 
            // lblMapWidthValue
            // 
            this.lblMapWidthValue.AutoSize = true;
            this.lblMapWidthValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMapWidthValue.Location = new System.Drawing.Point(53, 90);
            this.lblMapWidthValue.Name = "lblMapWidthValue";
            this.lblMapWidthValue.Size = new System.Drawing.Size(0, 17);
            this.lblMapWidthValue.TabIndex = 4;
            // 
            // lblMapWidth
            // 
            this.lblMapWidth.AutoSize = true;
            this.lblMapWidth.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMapWidth.Location = new System.Drawing.Point(6, 90);
            this.lblMapWidth.Name = "lblMapWidth";
            this.lblMapWidth.Size = new System.Drawing.Size(46, 17);
            this.lblMapWidth.TabIndex = 3;
            this.lblMapWidth.Text = "Rộng:";
            // 
            // trkbGridSize
            // 
            this.trkbGridSize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.trkbGridSize.Location = new System.Drawing.Point(9, 42);
            this.trkbGridSize.Maximum = 20;
            this.trkbGridSize.Minimum = 4;
            this.trkbGridSize.Name = "trkbGridSize";
            this.trkbGridSize.Size = new System.Drawing.Size(267, 45);
            this.trkbGridSize.SmallChange = 2;
            this.trkbGridSize.TabIndex = 2;
            this.trkbGridSize.TickFrequency = 2;
            this.trkbGridSize.Value = 20;
            this.trkbGridSize.ValueChanged += new System.EventHandler(this.trkbGridSize_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Kích Thước Led:";
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(163, 46);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(47, 23);
            this.btnDelete.TabIndex = 7;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnMap
            // 
            this.btnMap.Location = new System.Drawing.Point(57, 46);
            this.btnMap.Name = "btnMap";
            this.btnMap.Size = new System.Drawing.Size(47, 23);
            this.btnMap.TabIndex = 8;
            this.btnMap.Text = "MAP";
            this.btnMap.UseVisualStyleBackColor = true;
            this.btnMap.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.AutoScroll = true;
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Location = new System.Drawing.Point(312, 32);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(664, 526);
            this.panel2.TabIndex = 9;
            this.panel2.Scroll += new System.Windows.Forms.ScrollEventHandler(this.panel2_Scroll);
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(12, 88);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(39, 23);
            this.btnPlay.TabIndex = 3;
            this.btnPlay.Text = "Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(110, 88);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(47, 23);
            this.btnExport.TabIndex = 5;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnEnableEdit
            // 
            this.btnEnableEdit.Location = new System.Drawing.Point(163, 88);
            this.btnEnableEdit.Name = "btnEnableEdit";
            this.btnEnableEdit.Size = new System.Drawing.Size(93, 23);
            this.btnEnableEdit.TabIndex = 7;
            this.btnEnableEdit.Text = "Enable Edition";
            this.btnEnableEdit.UseVisualStyleBackColor = true;
            this.btnEnableEdit.Click += new System.EventHandler(this.btnEnableEdit_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(57, 88);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(47, 23);
            this.btnStop.TabIndex = 8;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(988, 570);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnMap);
            this.Controls.Add(this.btnEnableEdit);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnNewArea);
            this.Controls.Add(this.dgvListArea);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Main";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Resize += new System.EventHandler(this.Main_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.dgvListArea)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkbGridSize)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public FormEdit.overRidePanel panel1;
        private System.Windows.Forms.DataGridView dgvListArea;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.Button btnNewArea;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar trkbGridSize;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.ToolStripMenuItem enableEditionToolStripMenuItem;
        private System.Windows.Forms.Button btnMap;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox cbkAutoScroll;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveFileToolStripMenuItem;
        private System.Windows.Forms.Label lblMapWidth;
        private System.Windows.Forms.Label lblMapWidthValue;
        private System.Windows.Forms.Label lblMapHeightValue;
        private System.Windows.Forms.Label lblMapHeight;
        private System.Windows.Forms.Label lblMapTotalLedValue;
        private System.Windows.Forms.Label lblMapTotalLed;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnEnableEdit;
        private System.Windows.Forms.Button btnStop;
    }
}