using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CncBufferSpyClient.Properties;

namespace CncBufferSpyClient {
	public partial class MainForm : Form {
		private NamedPipeClientStream _pipeClient;
		private bool _die = false;
		private MemoryMappedFile _mappedFile1;
		private MemoryMappedFile _mappedFile2;
		private MemoryMappedViewAccessor _mapView1;
		private MemoryMappedViewAccessor _mapView2;
		private MemoryMappedViewAccessor _mapViewOfShownImage;
		private readonly object mapViewLock = new object();
		private readonly CancellationTokenSource _ctsPipeClient = new CancellationTokenSource();
		private PipeFrame _lastFrame;
		private PipeRequest _lastRequest;
		private readonly object requestLock = new object();
		private Bitmap _buffer1;
		private Bitmap _buffer2;
		private Size _buffersSize; // for cross-thread access
		private SurfaceType _surfaceRequestType = SurfaceType.Invalid;
		private uint _customRequestOffset;
		private int failCounter;


		public MainForm() {
			InitializeComponent();
			cbBufferType.DataSource = Enum.GetValues(typeof(SurfaceType)).OfType<SurfaceType>().Where(e => e != SurfaceType.Invalid).ToList();
			_lastFrame.SurfaceType = SurfaceType.Invalid;
		}

		private void buttonLaunch_Click(object sender, EventArgs e) {
			if (Interop.StartProcess(tbExecutablePath.Text)) {
				Log("Process started successfully\r\n");
				OpenPipeStream();
				UpdateProcessStatus();
			}
			else
				tbLog.AppendText("Failed to start process, aborting\r\n");
		}

		private void btnInject_Click(object sender, EventArgs e) {
			if (Interop.InjectToRunningProcess(tbExecutablePath.Text)) {
				Log("Injected successfully\r\n");
				OpenPipeStream();
				UpdateProcessStatus();
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
				_pipeClient = new NamedPipeClientStream(".", "cnc_buffer_spy", PipeDirection.InOut, PipeOptions.Asynchronous);
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
				Log("Pipe connection broken\r\n");
			}
		}

		private void HandleMessage(PipeMessage message) {
			if (message.MessageType == PipeMessageType.FrameAvailable) {

				lock (requestLock) {
					if (message.frame.DestBuffer != _lastRequest.DestinationBuffer) {
						// Drop this frame, it's from an older outstanding request for which we may no longer
						// have the associated map view handles.
						return;
					}
				}

				Bitmap img;
				failCounter = 0;
				lock (mapViewLock)
					img = ImageFromMappedFileView(message.frame);
				lock (canvas.ImageLock)
					canvas.Image = img;

				if (cbAutoRefresh.Checked)
					// request next image in alternating buffer
					RequestFrame(false);
				else
					Log($"Received frame #{message.frame.FrameNumber} of size {message.frame.Width}x{message.frame.Height} in buffer {(int)message.frame.DestBuffer}\r\n");
			}
			else if (message.MessageType == PipeMessageType.FrameRequestFailed) {
				failCounter++;
				if (failCounter < 2) {
					RequestFrame(false);
				}
				else {
					BeginInvoke((Action)delegate {
						Log("Outstanding request failed successively, disabling auto-refresh\r\n");
						cbAutoRefresh.Checked = false;
					});
				}
			}
		}

		private unsafe Bitmap ImageFromMappedFileView(PipeFrame frame) {
			if (!EnsureBuffersCompatible(frame))
				return null;

			var view = frame.DestBuffer == DestinationBuffer.Buffer1 ? _mapView1 : _mapView2;
			var bm = frame.DestBuffer == DestinationBuffer.Buffer1 ? _buffer1 : _buffer2;
			// Debug.Assert(bm != canvas.Image); // this assert would only work under a lock {}

			byte* ptr = null;
			view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
			var bmd = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

			bool jet = ckbJetColormap.Checked;
			int anchorX = (int)((frame.SourceBufferAnchor/2) % frame.Width);
			int anchorY = (int)((frame.SourceBufferAnchor/2) / frame.Width);

			if (frame.PixelFormat == BufferPixelFormat.Format16bppGrayscale)
				Blit16bppGrayscale(frame, ptr, bmd, anchorY, anchorX, jet);
			if (frame.PixelFormat == BufferPixelFormat.Format16bppRGB)
				Blit16bppRGB(frame, ptr, bmd, anchorY, anchorX);
			else if (frame.PixelFormat == BufferPixelFormat.Format8bpp)
				Blit8bpp(frame, ptr, bmd, anchorX, anchorY);

			view.SafeMemoryMappedViewHandle.ReleasePointer();
			bm.UnlockBits(bmd);

			lock (mapViewLock)
				_mapViewOfShownImage = view;
			_lastFrame = frame;

			return bm;
		}

		private unsafe void Blit8bpp(PipeFrame frame, byte* ptr, BitmapData bmd, int anchorX, int anchorY) {
			for (int row = 0; row < bmd.Height; row++) {
				byte* w = (byte*)bmd.Scan0.ToPointer() + row * bmd.Stride;
				byte* r = ptr + ((row + anchorY) % frame.Height) * frame.Width;
				int endCol = (int) ((anchorX + frame.Width - 1) % frame.Width);
				for (int col = anchorX; col != endCol;) {
					// low byte
					*w++ = r[col];
					*w++ = r[col];
					*w++ = r[col];

					col++;
					if (col == frame.Width) col = 0;
				}
			}
		}

		private static unsafe void Blit16bppGrayscale(PipeFrame frame, byte* ptr, BitmapData bmd, int anchorY, int anchorX, bool jet) {
			// determine range
			ushort* p = (ushort*) ptr;
			ushort min = 65535;
			ushort max = 0;
			if (jet) {
				ushort* end = p + frame.Width * frame.Height;
				while (p != end) {
					if (min > *p) min = *p;
					if (max < *p) max = *p;
					++p;
				}
			}

			float range = max - min;

			for (int row = 0; row < bmd.Height; row++) {
				byte* w = (byte*) bmd.Scan0.ToPointer() + row * bmd.Stride;
				byte* scan = ptr + 2 * ((row + anchorY) % frame.Height) * frame.Width;

				int endCol = (int) ((anchorX + frame.Width - 1) % frame.Width);
				for (int col = anchorX; col != endCol;) {
					if (jet) {
						ushort u = (ushort) ((scan[col * 2 + 1] << 8) | scan[col * 2]);
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
		}
		private static unsafe void Blit16bppRGB(PipeFrame frame, byte* ptr, BitmapData bmd, int anchorY, int anchorX) {
			for (int row = 0; row < bmd.Height; row++) {
				byte* w = (byte*) bmd.Scan0.ToPointer() + row * bmd.Stride;
				ushort* scan = (ushort*)(ptr + 2 * ((row + anchorY) % frame.Height) * frame.Width);

				int endCol = (int) ((anchorX + frame.Width - 1) % frame.Width);
				for (int col = anchorX; col != endCol;) {
					ushort bgr565 = scan[col];
					*w++ = (byte)((bgr565 & 0x001F) << 3); // b
					*w++ = (byte)((bgr565 & 0x07E0) >> 3); // g
					*w++ = (byte)((bgr565 & 0xF800) >> 8); // r

					col++;
					if (col == frame.Width) col = 0;
				}
			}
		}

		private bool EnsureBuffersCompatible(PipeFrame frame) {
			if (_mappedFile1 != null && _mapView1 != null &&
				_mappedFile2 != null && _mapView2 != null &&
				_buffer1 != null && _buffer2 != null &&
				_buffersSize.Width == frame.Width && _buffersSize.Height == frame.Height) {
				return true;
			}

			try {
				ReleaseHandles();

				lock (mapViewLock) {
					_mappedFile1 = MemoryMappedFile.OpenExisting("cnc_buffer_spy1", MemoryMappedFileRights.Read);
					_mapView1 = _mappedFile1.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

					_mappedFile2 = MemoryMappedFile.OpenExisting("cnc_buffer_spy2", MemoryMappedFileRights.Read);
					_mapView2 = _mappedFile2.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
				}

				lock (canvas.ImageLock) {
					canvas.Image = null;
					_buffer1 = new Bitmap((int)frame.Width, (int)frame.Height, PixelFormat.Format24bppRgb);
					_buffer2 = new Bitmap((int)frame.Width, (int)frame.Height, PixelFormat.Format24bppRgb);
				}

				_buffersSize = new Size((int)frame.Width, (int)frame.Height);
				return true;
			}
			catch {
				return false;
			}
		}

		private void ReleaseHandles() {
			// This is only done when allocated buffers are not (anymore) compatible with that just came in.
			lock (mapViewLock) {
				_mapViewOfShownImage = null;
				_mapView1?.Dispose();
				_mapView2?.Dispose();
				_mapView1 = _mapView2 = null;
				_mappedFile1?.Dispose();
				_mappedFile2?.Dispose();
				_mappedFile1 = _mappedFile2 = null;

				lock (canvas.ImageLock) {
					canvas.Image = null;
					_buffer1?.Dispose();
					_buffer2?.Dispose();
					_buffer1 = _buffer2 = null;
				}
			}
		}

		private void btnRequestFrame_Click(object sender, EventArgs e) {
			_surfaceRequestType = (SurfaceType)cbBufferType.SelectedIndex;
			RequestFrame(true); // always alternate buffers
		}

		private void btnRequestCustom_Click(object sender, EventArgs e) {
			_surfaceRequestType = SurfaceType.Custom;
			_customRequestOffset = Convert.ToUInt32(tbCustomOffset.Text, 16);
			RequestFrame(true);
		}
		private void RequestFrame(bool log) {
			if (_pipeClient == null) {
				Log("No pipe to write to!");
				return;
			}

			if (_surfaceRequestType != _lastFrame.SurfaceType) {
				// We are requesting a different surface type than the last one we received, which means hook DLL will likely
				// have to reallocate the shared memory, so we must release our handles to it, as it will be allocated under
				// the same name.
				ReleaseHandles();
			}

			// define request
			var msg = new PipeMessage();
			msg.MessageType = PipeMessageType.FrameRequest;
			msg.request = new PipeRequest();
			msg.request.DestinationBuffer = _lastRequest.DestinationBuffer == DestinationBuffer.Buffer1
				? DestinationBuffer.Buffer2
				: DestinationBuffer.Buffer1; // alternate
			msg.request.SurfaceType = _surfaceRequestType;
			msg.request.CustomOffset = _customRequestOffset;

			// convert to payload
			int size = Marshal.SizeOf(msg);
			byte[] buff = new byte[size];
			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(msg, ptr, true);
			Marshal.Copy(ptr, buff, 0, size);
			Marshal.FreeHGlobal(ptr);

			try {
				if (log) Log("Requesting next frame...");
				lock (requestLock) {
					_pipeClient.Write(buff, 0, size);
					_lastRequest = msg.request;
				}

				if (log) Log("done\r\n");
			}
			catch (ObjectDisposedException) { }
			catch (IOException) { }
		}

		private void cbAutoRefresh_CheckedChanged(object sender, EventArgs e) {
			if (cbAutoRefresh.Checked) {
				_surfaceRequestType = (SurfaceType)cbBufferType.SelectedIndex;
				RequestFrame(false);
			}
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
			_die = true;
			_ctsPipeClient.Cancel();
			ReleaseHandles();
			Settings.Default.Save();
		}

		private unsafe void canvas_MouseMove(object sender, MouseEventArgs e) {
			// prerequisites check
			Point location;
			ushort bufValue = 0x5A5A;
			lock (mapViewLock) {
				if (_mapViewOfShownImage == null)
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
				ptr += idx * _lastFrame.BytesPerPixel;
				if (_lastFrame.BytesPerPixel == 2)
					bufValue = (ushort)((*(ptr+1) << 8) | *(ptr+0));
				else if (_lastFrame.BytesPerPixel == 1)
					bufValue = *ptr;
			}

			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Mouse: ({0},{1}) ", location.X, location.Y);
			sb.AppendFormat(" -- buffer value:  {0} / 0x{0:X4}", bufValue);
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
			bool newRequest = cbAutoRefresh.Checked;
			_surfaceRequestType = (SurfaceType)cbBufferType.SelectedIndex;
			if (newRequest) {
				RequestFrame(true);
			}
		}

		private void timer_Tick(object sender, EventArgs e) {
			UpdateProcessStatus();
		}

		private void UpdateProcessStatus() {
			IntPtr handle = IntPtr.Zero;

			// Determine if process is running
			foreach (var p in Process.GetProcesses()) {
				// need to leave managed land here in order to avoid win32 exceptions if looking into processes we shouldn't
				handle = Interop.OpenProcess(0x0400 | 0x0010, false, p.Id);
				if (handle == IntPtr.Zero)
					continue;

				var filename = new StringBuilder(4096);
				if (Interop.GetModuleFileNameEx(handle, IntPtr.Zero, filename, filename.Capacity) > 0) {
					if (filename.ToString().Equals(tbExecutablePath.Text, StringComparison.InvariantCultureIgnoreCase)) {
						break; // intentially leaving handle open to enumerate modules!
					}
				}
				Interop.CloseHandle(handle);
			}
			lblRunning.Text = handle != IntPtr.Zero ? "RUNNING" : "NOT RUNNING";
			lblRunning.ForeColor = handle != IntPtr.Zero ? Color.Green : Color.Red;

			// Determine if DLL to be injected is loaded in process
			bool injected = false;

			if (handle != IntPtr.Zero) {
				string expectedModuleName =
					Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
						"CncBufferSpyHook.dll"));

				IntPtr[] hMods = new IntPtr[1024];
				GCHandle gch = GCHandle.Alloc(hMods, GCHandleType.Pinned); // Don't forget to free this later
				IntPtr pModules = gch.AddrOfPinnedObject();
				uint uiSize = (uint)(Marshal.SizeOf(typeof(IntPtr)) * hMods.Length);
				if (Interop.EnumProcessModules(handle, pModules, uiSize, out uint cbNeeded)) {
					long uiTotalNumberofModules = cbNeeded / Marshal.SizeOf(typeof(IntPtr));

					var sb = new StringBuilder(4096);
					for (int i = 0; i < (int)uiTotalNumberofModules; i++) {
						Interop.GetModuleFileNameEx(handle, hMods[i], sb, sb.Capacity);
						if (Path.GetFullPath(sb.ToString()).Equals(expectedModuleName)) {
							injected = true;
							break;
						}
					}
				}
				gch.Free();
				Interop.CloseHandle(handle);
			}

			lblInjected.Text = injected ? "INJECTED" : "NOT INJECTED";
			lblInjected.ForeColor = injected ? Color.Green : Color.Red;

			lblConnected.Text = _pipeClient?.CanRead == true ? "CONNECTED" : "NOT CONNECTED";
			lblConnected.ForeColor = _pipeClient?.CanRead == true ? Color.Green : Color.Red;
		}

	}
}
