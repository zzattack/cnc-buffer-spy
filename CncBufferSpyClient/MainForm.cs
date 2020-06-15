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
		private PipeFrame _lastFrame;
		private PipeBuffer _lastBuffer;
		private Bitmap _buffer1;
		private Bitmap _buffer2;
		private Size _buffersSize; // for cross-thread access
		private BufferType _bufferRequestType; // for cross-thread access


		public MainForm() {
			InitializeComponent();
			cbBufferType.DataSource = Enum.GetNames(typeof(BufferType));
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

					if (bytesRead == Marshal.SizeOf<PipeMessage>()) {
						// assume that it is, in fact, a pipe_msg
						GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
						PipeMessage message = (PipeMessage) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(PipeMessage));
						handle.Free();
						HandleMessage(message);
					}

				}
			}
			catch {
				Log("Failed to connect to named pipe\r\n");
			}
		}

		private void HandleMessage(PipeMessage message) {
			if (message.MessageType == PipeMessageType.FrameAvailable) {
				canvas.Image = ImageFromMappedFileView(message.frame);
				_lastBuffer = message.frame.buffer;

				if (cbAutoRefresh.Checked)
					// request next image in alternating buffer
					RequestFrame(message.frame.buffer == PipeBuffer.Buffer1 ? PipeBuffer.Buffer2 : PipeBuffer.Buffer1, false);
				else
					Log($"Received frame #{message.frame.framenr} of size {message.frame.Width}x{message.frame.Height} in buffer {(int)message.frame.buffer}\r\n");
			}
		}

		private unsafe Image ImageFromMappedFileView(PipeFrame frame) {
			if (!EnsureBuffersCompatible(frame))
				return null;

			var view = frame.buffer == PipeBuffer.Buffer1 ? _mapView1 : _mapView2;
			var bm = frame.buffer == PipeBuffer.Buffer1 ? _buffer1 : _buffer2;

			byte* ptr = null;
			view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
			var bmd = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

			// determine range
			ushort* p = (ushort*)ptr;
			ushort min = 65535;
			ushort max = 0;
			ushort* end = p + frame.Width * frame.Height;
			while (p != end) {
				if (min > *p) min = *p;
				if (max < *p) max = *p;
				++p;
			}
			float range = max - min;

			bool jet = ckbJetColormap.Checked;
			int anchor_x = (int)((frame.anchor_offset/2) % frame.Width);
			int anchor_y = (int)((frame.anchor_offset/2) / frame.Width);
			for (int row = 0; row < bmd.Height; row++) {
				byte* w = (byte*)bmd.Scan0.ToPointer() + row * bmd.Stride;
				byte* scan = ptr + 2 * ((row + anchor_y) % frame.Height) * frame.Width;

				int endCol = (int)((anchor_x + frame.Width - 1) % frame.Width);
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
					if (col == frame.Width) col = 0;
				}
			}

			view.SafeMemoryMappedViewHandle.ReleasePointer();
			bm.UnlockBits(bmd);

			lock (mapViewLock)
				_mapViewOfShownImage = view;
			_lastFrame = frame;

			return bm;
		}

		private bool EnsureBuffersCompatible(PipeFrame frame) {
			if (_mappedFile1 != null && _mapView1 != null &&
				_mappedFile2 != null && _mapView2 != null &&
				_buffer1 != null && _buffer2 != null &&
				_buffersSize.Width == frame.Width && _buffersSize.Height == frame.Height) {
				return true;
			}

			try {
				_mapView1?.Dispose();
				_mapView2?.Dispose();
				_mappedFile1?.Dispose();
				_mappedFile2?.Dispose();
				canvas.Image = null;
				_buffer1?.Dispose();
				_buffer2?.Dispose();

				_mappedFile1 = MemoryMappedFile.OpenExisting("cnc_buffer_spy1", MemoryMappedFileRights.Read);
				_mapView1 = _mappedFile1.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
				_buffer1 = new Bitmap((int)frame.Width, (int)frame.Height, PixelFormat.Format24bppRgb);

				_mappedFile2 = MemoryMappedFile.OpenExisting("cnc_buffer_spy2", MemoryMappedFileRights.Read);
				_mapView2 = _mappedFile2.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
				_buffer2 = new Bitmap((int)frame.Width, (int)frame.Height, PixelFormat.Format24bppRgb);

				_buffersSize = new Size((int)frame.Width, (int)frame.Height);
				return true;
			}
			catch {
				return false;
			}
		}

		private void btnRequestFrame_Click(object sender, EventArgs e) {
			RequestFrame(_lastBuffer == PipeBuffer.Buffer1 ? PipeBuffer.Buffer2 : PipeBuffer.Buffer1, true); // always alternate buffers
		}

		private async void RequestFrame(PipeBuffer buffer, bool log) {
			if (_pipeClient == null) {
				Log("No pipe to write to!");
				return;
			}

			// define request
			var msg = new PipeMessage();
			msg.MessageType = PipeMessageType.FrameRequest;
			msg.request = new PipeRequest();
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
				RequestFrame(_lastBuffer == PipeBuffer.Buffer1 ? PipeBuffer.Buffer2 : PipeBuffer.Buffer1, false);
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
				if (location.X < 0 || location.Y < 0 || location.X >= _buffersSize.Width || location.Y >= _buffersSize.Height)
					return;

				// get data from mapped buffer
				byte* ptr = null;
				_mapViewOfShownImage.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
				int idx = location.X + location.Y * _buffersSize.Width;
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

		private void cbBufferType_SelectedIndexChanged(object sender, EventArgs e) {
			_bufferRequestType = (BufferType)cbBufferType.SelectedIndex;
		}
	}
}