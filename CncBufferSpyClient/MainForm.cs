using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CncBufferSpyClient {
	public partial class MainForm : Form {
		private NamedPipeClientStream _pipeClient;
		private bool _die = false;
		private MemoryMappedFile _mappedFile1;
		private MemoryMappedFile _mappedFile2;
		private MemoryMappedViewAccessor _mapView1;
		private MemoryMappedViewAccessor _mapView2;
		private MemoryMappedViewAccessor _mapViewOfShownImage;
		private object mapViewLock = new object();
		CancellationTokenSource _ctsPipeClient = new CancellationTokenSource();
		private pipe_frame _lastFrame;
		private Bitmap _buffer1;
		private Bitmap _buffer2;

		public MainForm() {
			InitializeComponent();
		}

		private void buttonLaunch_Click(object sender, EventArgs e) {
			if (Interop.StartProcess(tbExecutablePath.Text)) {
				Log("Process started successfully\r\n");
				OpenPipeStream();
			}
			else
				tbLog.AppendText("Failed to start process, aborting\r\n");
		}

		private void btnInject_Click(object sender, EventArgs e) {
			if (Interop.InjectToRunningProcess("game.exe")) {
				Log("Injected successfully\r\n");
				OpenPipeStream();
			}
			else
				Log("Failed to inject, is process not running, injection already placed, or process to inject of higher elevation level?");
		}

		private void btnConnect_Click(object sender, EventArgs e) {
			_pipeClient?.Dispose();
			OpenPipeStream();
		}

		private void OpenPipeStream() {
			if (_pipeClient?.IsConnected != true) {
				_pipeClient?.Dispose();
				_pipeClient = new NamedPipeClientStream(".", "cnc_buffer_spy", PipeDirection.InOut,
					PipeOptions.Asynchronous);
				Task.Run(() => RunPipeClient(_pipeClient));
			}
			else
				tbLog.AppendText("Not opening pipe stream because it is already running\r\n");
		}

		private async void RunPipeClient(NamedPipeClientStream pipe) {
			try {
				Log("Connecting to pipe stream...");
				await pipe.ConnectAsync(_ctsPipeClient.Token);
				Log("done\r\n");

				while (!_die) {
					byte[] buffer = new byte[512];
					int bytesRead = await pipe.ReadAsync(buffer, 0, buffer.Length, _ctsPipeClient.Token);

					if (bytesRead == Marshal.SizeOf<pipe_msg>()) {
						// assume that it is, in fact, a pipe_msg
						GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
						pipe_msg msg = (pipe_msg) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(pipe_msg));
						handle.Free();
						HandleMessage(msg);
					}

				}
			}
			catch {
				Log("Failed to connect to named pipe\r\n");
			}
		}

		private void HandleMessage(pipe_msg msg) {
			if (msg.msg_type == pipe_msg_type.frame_available) {
				canvas.Image = ImageFromMappedFileView(msg.frame);

				if (cbAutoRefresh.Checked)
					// request next image in alternating buffer
					RequestFrame(msg.frame.buffer == pipe_buffer.buffer1 ? pipe_buffer.buffer2 : pipe_buffer.buffer1, false);
				else
					Log($"Received frame #{msg.frame.framenr} of size {msg.frame.reso_h}x{msg.frame.reso_v} in buffer {(int)msg.frame.buffer}\r\n");
			}
		}

		private unsafe Image ImageFromMappedFileView(pipe_frame frame) {
			MemoryMappedViewAccessor view = null;
			Bitmap bm = null;
			if (frame.buffer == pipe_buffer.buffer1) {
				if (_mappedFile1 == null) {
					_mappedFile1 = MemoryMappedFile.OpenExisting("cnc_buffer_spy1", MemoryMappedFileRights.Read);
					_mapView1 = _mappedFile1.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
				}

				view = _mapView1;
				if (_buffer1 == null)
					_buffer1 = new Bitmap((int)frame.reso_h, (int)frame.reso_v, PixelFormat.Format24bppRgb);
				bm = _buffer1;
			}
			else if (frame.buffer == pipe_buffer.buffer2) {
				if (_mappedFile2 == null) {
					_mappedFile2 = MemoryMappedFile.OpenExisting("cnc_buffer_spy2", MemoryMappedFileRights.Read);
					_mapView2 = _mappedFile2.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
				}

				view = _mapView2;
				if (_buffer2 == null)
					_buffer2 = new Bitmap((int)frame.reso_h, (int)frame.reso_v, PixelFormat.Format24bppRgb);
				bm = _buffer2;
			}

			if (view == null || bm == null)
				return null;

			byte* ptr = null;
			view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);

			var bmd = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

			// determine range
			ushort* p = (ushort*)ptr;
			ushort min = 65535;
			ushort max = 0;
			ushort* end = p + frame.reso_h * frame.reso_v;
			while (p != end) {
				if (min > *p) min = *p;
				if (max < *p) max = *p;
				++p;
			}
			float range = max - min;
			
			bool jet = ckbJetColormap.Checked;
			int anchor_x = (int)((frame.anchor_offset/2) % frame.reso_h);
			int anchor_y = (int)((frame.anchor_offset/2) / frame.reso_h);
			for (int row = 0; row < bmd.Height; row++) {
				byte* w = (byte*)bmd.Scan0.ToPointer() + row * bmd.Stride;
				byte* scan = ptr + 2 * ((row + anchor_y) % frame.reso_v) * frame.reso_h;

				int endCol = (int)((anchor_x + frame.reso_h - 1) % frame.reso_h);
				for (int col = anchor_x; col != endCol;) {

					if (jet) {
						ushort u = (ushort)((scan[col * 2 + 1] << 8) | scan[col * 2]);
						float v = 2 * (u - min) / range;
						byte b = (byte) Math.Max(0, 255 * (1 - v));
						byte r = (byte) Math.Max(0, 255 * (v - 1));
						byte g = (byte) (255 - b - r);
						*w++ = g;
						*w++ = b;
						*w++ = r;
					}
					else {
						// low byte
						*w++ = scan[col * 2 + 0];
						*w++ = scan[col * 2 + 0];
						*w++ = scan[col * 2 + 0];
					}

					col++;
					if (col == frame.reso_h) col = 0;
				}
			}

			view.SafeMemoryMappedViewHandle.ReleasePointer();
			bm.UnlockBits(bmd);

			lock (mapViewLock)
				_mapViewOfShownImage = view;
			_lastFrame = frame;

			return bm;
		}

		private void btnRequestFrame_Click(object sender, EventArgs e) {
			RequestFrame(pipe_buffer.buffer1, true); // doesn't matter which buffer
		}

		private async void RequestFrame(pipe_buffer buffer, bool log) {
			if (_pipeClient == null) {
				Log("No pipe to write to!");
				return;
			}

			// define request
			var msg = new pipe_msg();
			msg.msg_type = pipe_msg_type.frame_request;
			msg.request = new pipe_request();
			msg.request.buffer = buffer;

			// convert to payload
			int size = Marshal.SizeOf(msg);
			byte[] buff = new byte[size];
			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(msg, ptr, true);
			Marshal.Copy(ptr, buff, 0, size);
			Marshal.FreeHGlobal(ptr);

			if (log) Log("Requesting next frame...");
			await _pipeClient.WriteAsync(buff, 0, size);
			if (log) Log("done\r\n");
		}

		private void cbAutoRefresh_CheckedChanged(object sender, EventArgs e) {
			if (cbAutoRefresh.Checked)
				RequestFrame(pipe_buffer.buffer1, false);
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
			_die = true;
			_ctsPipeClient.Cancel();
			_mapView1?.Dispose();
			_mapView2?.Dispose();
			_mappedFile1?.Dispose();
			_mappedFile2?.Dispose();
			_buffer1?.Dispose();
			_buffer2?.Dispose();
		}

		private unsafe void canvas_MouseMove(object sender, MouseEventArgs e) {
			// prerequisites check
			Point location;
			ushort bufValue;
			lock (mapViewLock) {
				if (_mapViewOfShownImage == null || canvas.Image == null)
					return;

				// bounds check
				var pixelLocationF = canvas.PointToImagePixel(e.Location);
				location = new Point((int)Math.Round(pixelLocationF.X, 0), (int)Math.Round(pixelLocationF.Y, 0));
				if (location.X < 0 || location.Y < 0 || location.X >= canvas.ImageSize.Width || location.Y >= canvas.ImageSize.Height)
					return;

				// get data from mapped buffer
				byte* ptr = null;
				_mapViewOfShownImage.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
				int idx = location.X + location.Y * canvas.ImageSize.Width;
				ptr += idx * 2;
				bufValue = (ushort)((*(ptr+1) << 8) | *(ptr+0));
			}

			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Mouse: ({0},{1}) ", location.X, location.Y);
			sb.AppendFormat(" -- depth value:  {0} / 0x{0:X4}", bufValue);
			//uint offset = (uint)(2 * (location.X + location.Y * _lastFrame.reso_h));
			//sb.AppendFormat(" -- address: 0x{0:X8}, offset: 0x{1:X8}, offset/2: {2:X8}", _lastFrame.frame_memory_start + offset, offset, offset / 2);
			toolStripLabel.Text = sb.ToString();
		}

		private void MainForm_KeyDown(object sender, KeyEventArgs e) {
			if (e.Control && e.KeyCode == Keys.NumPad0) {
				var focus = new PointF(canvas.Width / 2f, canvas.Height / 2f); // center of currently visible area
				canvas.ZoomToLevel(0, focus);
			}
			else if (e.Control && e.KeyCode == Keys.NumPad1) {
				canvas.ZoomToFit();
			}
		}
		private void Log(string msg) {
			if (tbLog.InvokeRequired) tbLog.BeginInvoke((Action<string>)Log, msg);
			else tbLog.AppendText(msg);
		}

	}
}