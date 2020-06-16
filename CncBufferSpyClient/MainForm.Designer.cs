namespace CncBufferSpyClient {
	partial class MainForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.btnLaunch = new System.Windows.Forms.Button();
			this.btnInject = new System.Windows.Forms.Button();
			this.btnRequestSingleFrame = new System.Windows.Forms.Button();
			this.tbExecutablePath = new System.Windows.Forms.TextBox();
			this.btnConnect = new System.Windows.Forms.Button();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.cbAutoRefresh = new System.Windows.Forms.CheckBox();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.toolStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.tbLog = new System.Windows.Forms.TextBox();
			this.ckbJetColormap = new System.Windows.Forms.CheckBox();
			this.cbBufferType = new System.Windows.Forms.ComboBox();
			this.lblBufferType = new System.Windows.Forms.Label();
			this.gbLayers = new System.Windows.Forms.GroupBox();
			this.gbProcess = new System.Windows.Forms.GroupBox();
			this.lblConnected = new System.Windows.Forms.Label();
			this.lblInjected = new System.Windows.Forms.Label();
			this.lblRunning = new System.Windows.Forms.Label();
			this.gbOffset = new System.Windows.Forms.GroupBox();
			this.tbCustomOffset = new System.Windows.Forms.TextBox();
			this.lblOffset = new System.Windows.Forms.Label();
			this.btnRequestCustom = new System.Windows.Forms.Button();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.canvas = new CncBufferSpyClient.ZoomableCanvas();
			this.statusStrip.SuspendLayout();
			this.gbLayers.SuspendLayout();
			this.gbProcess.SuspendLayout();
			this.gbOffset.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnLaunch
			// 
			this.btnLaunch.Location = new System.Drawing.Point(18, 19);
			this.btnLaunch.Name = "btnLaunch";
			this.btnLaunch.Size = new System.Drawing.Size(82, 28);
			this.btnLaunch.TabIndex = 1;
			this.btnLaunch.Text = "Launch";
			this.toolTip.SetToolTip(this.btnLaunch, "Launch executable, inject to process and connect to pipe stream");
			this.btnLaunch.UseVisualStyleBackColor = true;
			this.btnLaunch.Click += new System.EventHandler(this.buttonLaunch_Click);
			// 
			// btnInject
			// 
			this.btnInject.Location = new System.Drawing.Point(121, 19);
			this.btnInject.Name = "btnInject";
			this.btnInject.Size = new System.Drawing.Size(82, 28);
			this.btnInject.TabIndex = 2;
			this.btnInject.Text = "Inject";
			this.toolTip.SetToolTip(this.btnInject, "Inject DLL into already running process");
			this.btnInject.UseVisualStyleBackColor = true;
			this.btnInject.Click += new System.EventHandler(this.btnInject_Click);
			// 
			// btnRequestSingleFrame
			// 
			this.btnRequestSingleFrame.Location = new System.Drawing.Point(201, 24);
			this.btnRequestSingleFrame.Name = "btnRequestSingleFrame";
			this.btnRequestSingleFrame.Size = new System.Drawing.Size(108, 28);
			this.btnRequestSingleFrame.TabIndex = 4;
			this.btnRequestSingleFrame.Text = "Request one frame";
			this.btnRequestSingleFrame.UseVisualStyleBackColor = true;
			this.btnRequestSingleFrame.Click += new System.EventHandler(this.btnRequestFrame_Click);
			// 
			// tbExecutablePath
			// 
			this.tbExecutablePath.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::CncBufferSpyClient.Properties.Settings.Default, "executablePath", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.tbExecutablePath.Location = new System.Drawing.Point(18, 53);
			this.tbExecutablePath.Name = "tbExecutablePath";
			this.tbExecutablePath.Size = new System.Drawing.Size(286, 20);
			this.tbExecutablePath.TabIndex = 5;
			this.tbExecutablePath.Text = global::CncBufferSpyClient.Properties.Settings.Default.executablePath;
			// 
			// btnConnect
			// 
			this.btnConnect.Location = new System.Drawing.Point(222, 19);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(82, 28);
			this.btnConnect.TabIndex = 6;
			this.btnConnect.Text = "Connect";
			this.toolTip.SetToolTip(this.btnConnect, "Connect to already injected process");
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// cbAutoRefresh
			// 
			this.cbAutoRefresh.AutoSize = true;
			this.cbAutoRefresh.Location = new System.Drawing.Point(201, 58);
			this.cbAutoRefresh.Name = "cbAutoRefresh";
			this.cbAutoRefresh.Size = new System.Drawing.Size(83, 17);
			this.cbAutoRefresh.TabIndex = 7;
			this.cbAutoRefresh.Text = "Auto-refresh";
			this.cbAutoRefresh.UseVisualStyleBackColor = true;
			this.cbAutoRefresh.CheckedChanged += new System.EventHandler(this.cbAutoRefresh_CheckedChanged);
			// 
			// statusStrip
			// 
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel});
			this.statusStrip.Location = new System.Drawing.Point(0, 782);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(1077, 22);
			this.statusStrip.TabIndex = 8;
			this.statusStrip.Text = "statusStrip1";
			// 
			// toolStripLabel
			// 
			this.toolStripLabel.Name = "toolStripLabel";
			this.toolStripLabel.Size = new System.Drawing.Size(111, 17);
			this.toolStripLabel.Text = "Mouse over for info";
			this.toolStripLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tbLog
			// 
			this.tbLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbLog.Location = new System.Drawing.Point(12, 669);
			this.tbLog.Multiline = true;
			this.tbLog.Name = "tbLog";
			this.tbLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tbLog.Size = new System.Drawing.Size(1053, 110);
			this.tbLog.TabIndex = 9;
			// 
			// ckbJetColormap
			// 
			this.ckbJetColormap.AutoSize = true;
			this.ckbJetColormap.Checked = true;
			this.ckbJetColormap.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ckbJetColormap.Location = new System.Drawing.Point(58, 56);
			this.ckbJetColormap.Name = "ckbJetColormap";
			this.ckbJetColormap.Size = new System.Drawing.Size(86, 17);
			this.ckbJetColormap.TabIndex = 10;
			this.ckbJetColormap.Text = "Jet colormap";
			this.ckbJetColormap.UseVisualStyleBackColor = true;
			// 
			// cbBufferType
			// 
			this.cbBufferType.FormattingEnabled = true;
			this.cbBufferType.Location = new System.Drawing.Point(58, 29);
			this.cbBufferType.Name = "cbBufferType";
			this.cbBufferType.Size = new System.Drawing.Size(121, 21);
			this.cbBufferType.TabIndex = 11;
			this.cbBufferType.SelectedIndexChanged += new System.EventHandler(this.cbBufferType_SelectedIndexChanged);
			// 
			// lblBufferType
			// 
			this.lblBufferType.AutoSize = true;
			this.lblBufferType.Location = new System.Drawing.Point(17, 32);
			this.lblBufferType.Name = "lblBufferType";
			this.lblBufferType.Size = new System.Drawing.Size(33, 13);
			this.lblBufferType.TabIndex = 12;
			this.lblBufferType.Text = "Layer";
			// 
			// gbLayers
			// 
			this.gbLayers.Controls.Add(this.lblBufferType);
			this.gbLayers.Controls.Add(this.ckbJetColormap);
			this.gbLayers.Controls.Add(this.cbBufferType);
			this.gbLayers.Controls.Add(this.btnRequestSingleFrame);
			this.gbLayers.Controls.Add(this.cbAutoRefresh);
			this.gbLayers.Location = new System.Drawing.Point(340, 12);
			this.gbLayers.Name = "gbLayers";
			this.gbLayers.Size = new System.Drawing.Size(324, 103);
			this.gbLayers.TabIndex = 13;
			this.gbLayers.TabStop = false;
			this.gbLayers.Text = "Predefined layers";
			// 
			// gbProcess
			// 
			this.gbProcess.Controls.Add(this.lblConnected);
			this.gbProcess.Controls.Add(this.lblInjected);
			this.gbProcess.Controls.Add(this.lblRunning);
			this.gbProcess.Controls.Add(this.btnConnect);
			this.gbProcess.Controls.Add(this.btnLaunch);
			this.gbProcess.Controls.Add(this.btnInject);
			this.gbProcess.Controls.Add(this.tbExecutablePath);
			this.gbProcess.Location = new System.Drawing.Point(12, 12);
			this.gbProcess.Name = "gbProcess";
			this.gbProcess.Size = new System.Drawing.Size(322, 103);
			this.gbProcess.TabIndex = 14;
			this.gbProcess.TabStop = false;
			this.gbProcess.Text = "Game process";
			// 
			// lblConnected
			// 
			this.lblConnected.AutoSize = true;
			this.lblConnected.Location = new System.Drawing.Point(218, 80);
			this.lblConnected.Name = "lblConnected";
			this.lblConnected.Size = new System.Drawing.Size(100, 13);
			this.lblConnected.TabIndex = 9;
			this.lblConnected.Text = "NOT CONNECTED";
			// 
			// lblInjected
			// 
			this.lblInjected.AutoSize = true;
			this.lblInjected.Location = new System.Drawing.Point(119, 80);
			this.lblInjected.Name = "lblInjected";
			this.lblInjected.Size = new System.Drawing.Size(85, 13);
			this.lblInjected.TabIndex = 8;
			this.lblInjected.Text = "NOT INJECTED";
			// 
			// lblRunning
			// 
			this.lblRunning.AutoSize = true;
			this.lblRunning.Location = new System.Drawing.Point(17, 80);
			this.lblRunning.Name = "lblRunning";
			this.lblRunning.Size = new System.Drawing.Size(84, 13);
			this.lblRunning.TabIndex = 7;
			this.lblRunning.Text = "NOT RUNNING";
			// 
			// gbOffset
			// 
			this.gbOffset.Controls.Add(this.tbCustomOffset);
			this.gbOffset.Controls.Add(this.lblOffset);
			this.gbOffset.Controls.Add(this.btnRequestCustom);
			this.gbOffset.Location = new System.Drawing.Point(670, 12);
			this.gbOffset.Name = "gbOffset";
			this.gbOffset.Size = new System.Drawing.Size(324, 103);
			this.gbOffset.TabIndex = 14;
			this.gbOffset.TabStop = false;
			this.gbOffset.Text = "Surface at custom offset";
			// 
			// textBox1
			// 
			this.tbCustomOffset.Location = new System.Drawing.Point(84, 29);
			this.tbCustomOffset.Name = "tbCustomOffset";
			this.tbCustomOffset.Size = new System.Drawing.Size(100, 20);
			this.tbCustomOffset.TabIndex = 13;
			this.tbCustomOffset.Text = "0x12345678";
			// 
			// lblOffset
			// 
			this.lblOffset.AutoSize = true;
			this.lblOffset.Location = new System.Drawing.Point(17, 32);
			this.lblOffset.Name = "lblOffset";
			this.lblOffset.Size = new System.Drawing.Size(61, 13);
			this.lblOffset.TabIndex = 12;
			this.lblOffset.Text = "Offset (hex)";
			// 
			// btnRequestCustom
			// 
			this.btnRequestCustom.Location = new System.Drawing.Point(201, 24);
			this.btnRequestCustom.Name = "btnRequestCustom";
			this.btnRequestCustom.Size = new System.Drawing.Size(108, 28);
			this.btnRequestCustom.TabIndex = 4;
			this.btnRequestCustom.Text = "Request one frame";
			this.btnRequestCustom.UseVisualStyleBackColor = true;
			this.btnRequestCustom.Click += new System.EventHandler(this.btnRequestCustom_Click);
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 500;
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// canvas
			// 
			this.canvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.canvas.Image = null;
			this.canvas.Location = new System.Drawing.Point(12, 223);
			this.canvas.Name = "canvas";
			this.canvas.Size = new System.Drawing.Size(1053, 440);
			this.canvas.TabIndex = 0;
			this.canvas.Text = "zoomableCanvas1";
			this.canvas.VirtualMode = false;
			this.canvas.VirtualSize = new System.Drawing.Size(0, 0);
			this.canvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseMove);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1077, 804);
			this.Controls.Add(this.gbOffset);
			this.Controls.Add(this.gbProcess);
			this.Controls.Add(this.tbLog);
			this.Controls.Add(this.statusStrip);
			this.Controls.Add(this.canvas);
			this.Controls.Add(this.gbLayers);
			this.KeyPreview = true;
			this.Name = "MainForm";
			this.Text = "CNC Buffer Spy Client";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.gbLayers.ResumeLayout(false);
			this.gbLayers.PerformLayout();
			this.gbProcess.ResumeLayout(false);
			this.gbProcess.PerformLayout();
			this.gbOffset.ResumeLayout(false);
			this.gbOffset.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZoomableCanvas canvas;
		private System.Windows.Forms.Button btnLaunch;
		private System.Windows.Forms.Button btnInject;
		private System.Windows.Forms.Button btnRequestSingleFrame;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.TextBox tbExecutablePath;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.CheckBox cbAutoRefresh;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel toolStripLabel;
		private System.Windows.Forms.TextBox tbLog;
		private System.Windows.Forms.CheckBox ckbJetColormap;
		private System.Windows.Forms.ComboBox cbBufferType;
		private System.Windows.Forms.Label lblBufferType;
		private System.Windows.Forms.GroupBox gbLayers;
		private System.Windows.Forms.GroupBox gbProcess;
		private System.Windows.Forms.Label lblConnected;
		private System.Windows.Forms.Label lblInjected;
		private System.Windows.Forms.Label lblRunning;
		private System.Windows.Forms.GroupBox gbOffset;
		private System.Windows.Forms.TextBox tbCustomOffset;
		private System.Windows.Forms.Label lblOffset;
		private System.Windows.Forms.Button btnRequestCustom;
		private System.Windows.Forms.Timer timer;
	}
}

