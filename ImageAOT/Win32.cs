using System.Runtime.InteropServices;

namespace ImageAOT {
	public static class Win32 {
		public const int WM_SYSCOMMAND = 0x112;
		public const int WM_SIZING = 0x0214;

		public const int WMSZ_LEFT = 1;
		public const int WMSZ_RIGHT = 2;
		public const int WMSZ_TOP = 3;
		public const int WMSZ_TOPLEFT = 4;
		public const int WMSZ_TOPRIGHT = 5;
		public const int WMSZ_BOTTOM = 6;
		public const int WMSZ_BOTTOMLEFT = 7;
		public const int WMSZ_BOTTOMRIGHT = 8;

		public const int MF_STRING = 0x0;
		public const int MF_SEPARATOR = 0x800;

		public enum WindowSide {
			Left = WMSZ_LEFT,
			Right = WMSZ_RIGHT,
			Top = WMSZ_TOP,
			Bottom = WMSZ_BOTTOM,
			TopLeft = WMSZ_TOPLEFT,
			TopRight = WMSZ_TOPRIGHT,
			BottomLeft = WMSZ_BOTTOMLEFT,
			BottomRight = WMSZ_BOTTOMRIGHT
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT {
			public int left, top, right, bottom;
		}

		public static bool IsResizingMessage(int msg) {
			return msg == WM_SIZING;
		}
		public static Rectangle GetRectangle(IntPtr lParam) {
			RECT r = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT))!;
			return new(r.left, r.top, r.right - r.left, r.bottom - r.top);
		}
		public static void SetRectangle(IntPtr lParam, Rectangle rect) {
			RECT r = new() {
				left = rect.Left,
				top = rect.Top,
				right = rect.Right,
				bottom = rect.Bottom
			};
			Marshal.StructureToPtr(r, lParam, true);
		}
		public static WindowSide GetWindowSide(IntPtr wParam) {
			return (WindowSide)wParam.ToInt32();
		}

		[DllImport("user32")]
		public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
		[DllImport("user32")]
		public static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);
		[DllImport("user32")]
		public static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);
	}
}
