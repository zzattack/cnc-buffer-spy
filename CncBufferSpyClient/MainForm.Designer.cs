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
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.cbAutoRefresh = new System.Windows.Forms.CheckBox();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.toolStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.tbLog = new System.Windows.Forms.TextBox();
			this.ckbJetColormap = new System.Windows.Forms.CheckBox();
			this.cbBufferType = new System.Windows.Forms.ComboBox();
			this.lblBufferType = new System.Windows.Forms.Label();
			this.canvas = new CncBufferSpyClient.ZoomableCanvas();
			this.statusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnLaunch
			// 
			this.btnLaunch.Location = new System.Drawing.Point(35, 12);
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
			this.btnInject.Location = new System.Drawing.Point(123, 12);
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
			this.btnRequestSingleFrame.Location = new System.Drawing.Point(362, 12);
			this.btnRequestSingleFrame.Name = "btnRequestSingleFrame";
			this.btnRequestSingleFrame.Size = new System.Drawing.Size(108, 28);
			this.btnRequestSingleFrame.TabIndex = 4;
			this.btnRequestSingleFrame.Text = "Request one frame";
			this.btnRequestSingleFrame.UseVisualStyleBackColor = true;
			this.btnRequestSingleFrame.Click += new System.EventHandler(this.btnRequestFrame_Click);
			// 
			// tbExecutablePath
			// 
			this.tbExecutablePath.Location = new System.Drawing.Point(35, 46);
			this.tbExecutablePath.Name = "tbExecutablePath";
			this.tbExecutablePath.Size = new System.Drawing.Size(258, 20);
			this.tbExecutablePath.TabIndex = 5;
			this.tbExecutablePath.Text = "C:\\Westwood\\RA2\\gamemd.exe";
			// 
			// btnConnect
			// 
			this.btnConnect.Location = new System.Drawing.Point(211, 12);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(82, 28);
			this.btnConnect.TabIndex = 6;
			this.btnConnect.Text = "Connect";
			this.toolTip.SetToolTip(this.btnConnect, "Connect to already injected process");
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// checkBox1
			// 
			this.checkBox1.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point(489, 15);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(74, 23);
			this.checkBox1.TabIndex = 7;
			this.checkBox1.Text = "Auto-refresh";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// cbAutoRefresh
			// 
			this.cbAutoRefresh.Appearance = System.Windows.Forms.Appearance.Button;
			this.cbAutoRefresh.AutoSize = true;
			this.cbAutoRefresh.Location = new System.Drawing.Point(488, 15);
			this.cbAutoRefresh.Name = "cbAutoRefresh";
			this.cbAutoRefresh.Size = new System.Drawing.Size(74, 23);
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
			this.ckbJetColormap.Location = new System.Drawing.Point(600, 19);
			this.ckbJetColormap.Name = "ckbJetColormap";
			this.ckbJetColormap.Size = new System.Drawing.Size(86, 17);
			this.ckbJetColormap.TabIndex = 10;
			this.ckbJetColormap.Text = "Jet colormap";
			this.ckbJetColormap.UseVisualStyleBackColor = true;
			// 
			// cbBufferType
			// 
			this.cbBufferType.FormattingEnabled = true;
			this.cbBufferType.Location = new System.Drawing.Point(829, 17);
			this.cbBufferType.Name = "cbBufferType";
			this.cbBufferType.Size = new System.Drawing.Size(121, 21);
			this.cbBufferType.TabIndex = 11;
			this.cbBufferType.SelectedIndexChanged += new System.EventHandler(this.cbBufferType_SelectedIndexChanged);
			// 
			// lblBufferType
			// 
			this.lblBufferType.AutoSize = true;
			this.lblBufferType.Location = new System.Drawing.Point(788, 20);
			this.lblBufferType.Name = "lblBufferType";
			this.lblBufferType.Size = new System.Drawing.Size(33, 13);
			this.lblBufferType.TabIndex = 12;
			this.lblBufferType.Text = "Layer";
			// 
			// canvas
			// 
			this.canvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.canvas.Image = null;
			this.canvas.Location = new System.Drawing.Point(12, 72);
			this.canvas.Name = "canvas";
			this.canvas.Size = new System.Drawing.Size(1053, 591);
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
			this.Controls.Add(this.lblBufferType);
			this.Controls.Add(this.cbBufferType);
			this.Controls.Add(this.ckbJetColormap);
			this.Controls.Add(this.tbLog);
			this.Controls.Add(this.statusStrip);
			this.Controls.Add(this.cbAutoRefresh);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.btnConnect);
			this.Controls.Add(this.tbExecutablePath);
			this.Controls.Add(this.btnRequestSingleFrame);
			this.Controls.Add(this.btnInject);
			this.Controls.Add(this.btnLaunch);
			this.Controls.Add(this.canvas);
			this.KeyPreview = true;
			this.Name = "MainForm";
			this.Text = "CNC Buffer Spy Client";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
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
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.CheckBox cbAutoRefresh;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel toolStripLabel;
		private System.Windows.Forms.TextBox tbLog;
		private System.Windows.Forms.CheckBox ckbJetColormap;
		private System.Windows.Forms.ComboBox cbBufferType;
		private System.Windows.Forms.Label lblBufferType;
	}
}

