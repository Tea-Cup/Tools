using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace ImageAOT {
	public partial class Form1 : Form {
		private const int SYSMENU_OPEN_ID = 1;
		private const int SYSMENU_RECENT_START_ID = 2;
		private string? Filename { get; set; }
		private Dictionary<int, string> RecentIDs { get; } = new();

		public Form1() {
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e) {
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 1) {
				SetFilename(args[1]);
				return;
			}

			base.OnShown(e);
		}

		private void SetRecentRectangle() {
			if (Filename is null) return;
			RecentEntry? entry = Settings.GetRecent(Filename);
			if (entry is null) return;
			Location = entry.Point;
			Size = entry.Size;
		}
		private void SaveRecentRectangle() {
			if (Filename is null) return;
			Settings.SetRecent(Filename, Left, Top, Width, Height);
		}

		private void SetFilename(string filename) {
			SaveRecentRectangle();
			if (!TryGetImage(filename, out Bitmap? bmp, out string? msg)) {
				MessageBox.Show(this, msg + "\nPath:\n" + filename, "ImageOnTop", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
				return;
			}
			pictureBox1.Image = bmp;
			Text = Path.GetFileName(filename);
			TopMost = true;
			Filename = filename;
			SetRecentRectangle();
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated(e);
			IntPtr hSysMenu = Win32.GetSystemMenu(Handle, false);
			Win32.AppendMenu(hSysMenu, Win32.MF_SEPARATOR, 0, string.Empty);
			Win32.AppendMenu(hSysMenu, Win32.MF_STRING, SYSMENU_OPEN_ID, "&Open...");
			int id = SYSMENU_RECENT_START_ID;
			foreach (RecentEntry entry in Settings.Recent) {
				Win32.AppendMenu(hSysMenu, Win32.MF_STRING, id, entry.Path);
				RecentIDs[id] = entry.Path;
				id++;
			}
		}

		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
			if (m.Msg == Win32.WM_SYSCOMMAND) {
				if (m.WParam.ToInt32() == SYSMENU_OPEN_ID) {
					OpenFileDialog ofd = new() {
						CheckFileExists = true,
						CheckPathExists = true,
						InitialDirectory = Settings.LastLocation,
						Multiselect = false,
						Title = "Open image"
					};
					if (ofd.ShowDialog() == DialogResult.OK) {
						SetFilename(ofd.FileName);
						Settings.LastLocation = Path.GetDirectoryName(ofd.FileName) ?? Environment.CurrentDirectory;
					}
				}
				if (RecentIDs.TryGetValue(m.WParam.ToInt32(), out string? path)) {
					SetFilename(path);
				}
			}
		}

		protected override void OnClosing(CancelEventArgs e) {
			SaveRecentRectangle();
			base.OnClosing(e);
		}

		static bool TryGetImage(string filename, [MaybeNullWhen(false)] out Bitmap bmp, [MaybeNullWhen(true)] out string message) {
			FileInfo fi = new(filename);
			if (!fi.Exists) {
				bmp = null;
				message = "File does not exist";
				return false;
			}

			try {
				bmp = new(filename);
				message = null;
				return true;
			} catch (Exception ex) {
				bmp = null;
				message = "Error loading image:\n" + ex.Message;
				return false;
			}
		}
	}
}