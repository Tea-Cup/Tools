# ImageOnTop

Open an image in an Always On Top Window.

Can accept image filepath via command line arguments, or can be launched as is.

Window icon (system menu) has additional entries:

- `Open...` - Opens a dialog for selecting a file to open.
- Recent files - Those are saved in registry (`HKCU\SOFTWARE\Foxy\IOT\Recent`) along with window location and size.

Last open file dialog directory is saved in registry (`HKCU\SOFTWARE\Foxy\IOT`), so it remembers last location.

File is saved to recents on window close.

Only valid (file exists) entries are loaded to menu.

No limit on those yet. The list can get big...
