# Tools

Set of tool aliases I'm too bothered to remember.

## How to use

- Put it in `%PATH%`, rename it something like `t.exe`
- Put [ffmpeg.exe](https://www.ffmpeg.org/), [youtube-dl.exe](http://ytdl-org.github.io/youtube-dl/) and [iot.exe](https://github.com/Tea-Cup/Tools/tree/main/ImageAOT) nearby.
- If needed, create a [t.config.ini](https://github.com/Tea-Cup/Tools/blob/main/Main/config.ini) settings file in the same folder and adjust paths.
- To download youtube audio, execute `t yt -a https://www.youtube.com/watch?v=dQw4w9WgXcQ`
- To use (some of) ffmpeg, execute `t ff "Rick Astley - Never Gonna Give You Up (Official Music Video)-dQw4w9WgXcQ.opus" --filter="compand,showwavespic=s=640x120" --format="png"`
- To view image always on top, execute `t iot "Rick Astley - Never Gonna Give You Up (Official Music Video)-dQw4w9WgXcQ.png

Keep in mind, both short and long CLI arguments require being connected with its value with an equals-sign (=)  
So don't do `-b 100k` or `--bitrate 100k`, do `-b=100k` or `--bitrate=100k`.  
Boolean (flag) arguments dont require a value.

Most resulting command line patterns can be changed in config. See comments there for specifics.

## Command Line

### Base

- `list` - Display loaded tools list.
- `help` or `?` or without parameters - Display help info.
- `help` or `?` and then tool command - Display info about a tool with this command.
- \<tool command> ... - Execute tool.
- `--config=<path>` - To specify config file path. (Default: `./EXE.config.ini` where `EXE` is current .exe filename)

### Youtube-DL plugin

Aliases: `yt`, `ytdl`, `yt-dl`, `youtube`, `youtubedl`, `youtube-dl`  
Download media from YouTube.

Options:

- `-a` or `--only-audio` - Download only audio.
- ... - list of YouTube URL's

### FFMpeg plugin

Aliases: `ff`, `ffmpeg`  
FFMpeg short commands.

Options:

- `--format` - Resulting format (extension with or without a leading period. Ignored if `--output` is specified. (Default: .mp4)
- `-o`, `--output` - Output filepath.
- `-b`, `--bitrate` - Target **video** bitrate. (-b:v in ffmpeg)
- `-f`, `--filter` - Filter string. (--filter_complex in ffmpeg)
- `-s`, `--scale` - Target scale setting. Ignore if `--filter` is used. (-vf "scale=..." in ffmpeg)
- `-i`, `--inspect` - Inspect input file. All other options are ignore. (ffprobe used instead of ffmpeg)
- ... - Input filepath.

### Image On Top plugin

Aliases: `iot`, `imageontop`
Open an image in an Always On Top Window.

Accepts no options but input filename (optional).  
See [IOT README.md](https://github.com/Tea-Cup/Tools/blob/main/ImageAOT/README.md) for help on that.

## How it works

Input command line is parsed and applied on a pattern set in config. Result is the launched in shell. That's it.
