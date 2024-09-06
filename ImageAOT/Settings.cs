using Microsoft.Win32;

namespace ImageAOT {
	public static class Settings {
		private static RegistryKey RootKey { get; } = Registry.CurrentUser.CreateOrOpen(@"SOFTWARE\Foxy\IOT");
		private static RegistryKey RecentKey { get; } = RootKey.CreateOrOpen("Recent");
		public static string LastLocation {
			get {
				if (!RootKey.IsValueOfKind("last_location", RegistryValueKind.String))
					RootKey.DeleteValue("last_location", false);
				string? v = (string?)RootKey.GetValue("last_location");
				return v ?? Environment.CurrentDirectory;
			}
			set {
				RootKey.SetValue("last_location", value, RegistryValueKind.String);
			}
		}
		public static IEnumerable<RecentEntry> Recent {
			get {
				foreach (string path in RecentKey.GetValueNames()) {
					RecentEntry? entry = GetRecent(path);
					if(entry is null || !File.Exists(entry.Path)) {
						RecentKey.DeleteValue(path, false);
					} else {
						yield return entry;
					}
				}
			}
			set {
				RecentEntry[] entries = value.ToArray();
				foreach (string path in RecentKey.GetValueNames()) RecentKey.DeleteValue(path, false);
				foreach (RecentEntry entry in entries) {
					SetRecent(entry);
				}
			}
		}

		public static void SetRecent(RecentEntry entry) {
			RecentKey.SetValue(entry.Path, entry.RectangleString, RegistryValueKind.String);
		}
		public static void SetRecent(string path, int x, int y, int w, int h) {
			SetRecent(new(path, x, y, w, h));
		}
		public static RecentEntry? GetRecent(string path) {
			if(!RootKey.IsValueOfKind("last_location", RegistryValueKind.String)) return null;
			string? v = (string?)RecentKey.GetValue(path);
			if (v is null) return null;

			string[] split = v.Split(',');
			if (split.Length != 4) {
				return null;
			}

			if (!int.TryParse(split[0], out int x) ||
				!int.TryParse(split[1], out int y) ||
				!int.TryParse(split[2], out int w) ||
				!int.TryParse(split[3], out int h)
			) {
				return null;
			}

			return new(path, x, y, w, h);
		}

		private static RegistryKey CreateOrOpen(this RegistryKey key, string path) {
			return key.OpenSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree)
				?? key.CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
		}
		private static bool IsValueExists(this RegistryKey key, string path) {
			return key.GetValue(path) is not null;
		}
		private static bool IsValueOfKind(this RegistryKey key, string name, RegistryValueKind kind) {
			return key.IsValueExists(name) && key.GetValueKind(name) == kind;
		}
	}
	public record RecentEntry(string Path, int X, int Y, int Width, int Height) {
		public Rectangle Rectangle => new(X, Y, Width, Height);
		public Point Point => new(X, Y);
		public Size Size => new(Width, Height);
		public string RectangleString => $"{X},{Y},{Width},{Height}";
	}
}
