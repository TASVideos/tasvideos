public static class TestPages
{
	public const string EncoderGuidelines = """
!!! Guidelines for making multimedia files for publication

This page explains general guidelines for making multimedia files up to the standards of the TASVideos website. A more detailed "how to" can be found at the [Encoding Guide].

%%TOC%%

!! Introduction

TASVideos (formerly Nesvideos) was born from the need to provide good-quality multimedia files of tool-assisted speedruns. We take quality very seriously, and strive to produce files that are pleasant to watch and that are as small as possible.
We also stress the informative value of the movies, and thus we have a few guidelines, that are mandatory reading for anyone who wants to make MP4 files to be published on this site.

As set out in our [WelcomeToTASVideos|introduction page] and [=MovieRules.html#GamesMustBeReal|parts of the Movie Rules], our runs are intended to appear as though they could be played on the original hardware (if not [ConsoleVerification/Movies|actually capable of being played back on the original hardware]). Thus, one core guideline for our encodes is to __appear, as closely as possible, as though the run was played on the original hardware__.

The other and possibly more fundamental core guideline here is __annoy the viewer the least__.  This implies the following constraints, between which we aim for the best possible compromise:
* Quality: The video should look and sound as close to the pre-encoding input (i.e. raw emulator output) as possible.
* Size: The video file should be as small as possible; this is enforced with per-platform ratio limits, as described below.

There are two additional key points that our videos must adhere to:
* The video content must label the movie as tool-assisted so as to prevent the misconception that it is an unassisted speedrun (a significant amount of controversy has arisen over such misconceptions in the past). Normally, this also includes a pointer to the TASVideos website because it is currently the most reliable source of TAS-related information in the Internet.
* The video content must label the player by name and the TASVideos website as the source. This information should be presented in such a fashion that it cannot be removed from the movie without extraordinary effort; this mostly means that labeling in the form of metadata or "soft subs" is not sufficient, and "hard subs" must be used.
----
!! Emulation settings

In keeping with the "as close as possible to hardware output" principle, settings that cause video or audio to deviate significantly from an actual console's output (as compared to not using the settings) should not be used. Examples of these settings include FCEUX's "allow more than 8 sprites per scanline" or Gens's "PSG High Quality".

These requirements are relaxed significantly when dealing with 3D games, especially on emulators that cannot accurately replicate the original console's output (example: Dolphin). In these cases, it is permitted to use anti-aliasing, anisotropic filtering, and other texture filtering settings, even if they are not available on the original console.
----
!! Video capture

Video must be captured using a recorder that will not skip or duplicate frames;
our emulators normally provide this functionality natively.  Most screen recorders
and other external program (such as Camtasia or Fraps) are not acceptable because
they do not know when the contents of the emulator window change and hence cannot
capture all frames, nor can they slow down the capture in the event of an encoding or I/O stall.  When an emulator has no internal capture or an extremely poor internal capture (such as PCSX or PrBoom); [http://www.farb-rausch.de/~fg/kkapture/|.kkapture] is allowed.  It can throttle recording speeds correctly for slow CPUs and comes much closer to frame accuracy than most screen capture tools, but is still not allowed for systems that have working internal capture.

The [Encoding Guide] contains links on how to configure various
emulators for video/audio capture.

! Frame rate

The movie should be captured at full frame rate (approximately 60fps for NTSC
consoles and 50fps for PAL) so that each unique frame that is rendered by the hardware is displayed precisely once - i.e. frames cannot be skipped.  If the emulator's built-in video dumping cannot satisfy this requirement (such as VBA), then the best result that can be reasonably obtained is preferred.

! Resolution (image size)

The encode must at least have a 2-to-1 scaling ratio, except for games that output at a height of 400 pixels or above natively. For handhelds, it is allowed to have a 4-to-1 scaling ratio.

Some consoles (currently, Genesis, PSX and DOS) display content at multiple resolutions. In this case, an exception is allowed to allow content to be scaled
either to the resolution most commonly used in the run (ensuring that the video
content most often seen is free of scaling artifacts) or to the largest width and 
height of all present video content (such that no data is lost in scaling).

If an emulator is not capable of producing output that matches pixels on the actual emulated system 1:1, then this requirement is relaxed (example Mupen, Dolphin).

For those consoles which are intended to be displayed on a TV or on CRT
monitors, the video must specify the correct display aspect ratio (often 4:3), as the video output for those consoles is normally displayed at that aspect ratio. Handhelds
or other consoles not intended for TV/CRT display don't need AR correction, because
their resolution is always fixed to match the display matrix pixel-for-pixel without any kind of stretching involved; thus, any AR that doesn't match the native
resolution will distort the image.

! Duration of the video

The video must start at the beginning of the input file (normally console start-up) and must include the full ending (with at least one full loop of the ending song for those games that have them).

Some games contain secret messages of different kinds that appear in some time of waiting after the credits end. It's at the discretion of the publisher/encoder to include secret messages. Here's the list of games known to contain such messages: 
* NES Lagrange Point
* SNES Secret of Evermore
* Genesis Chakan
* NES Takeshi no Chousenjou
----
!! Logo, subtitles, and other extras
! Logo

Gameplay should be prefaced by a ~2 second logo, which displays prominently the
website [=|https://TASVideos.org/] and the text "This is a tool-assisted emulator movie"
or a close variant thereof.  Logos are additionally meant to identify the encoder,
so personalized content of some sort is advisable so long as it appropriate and does not overshadow
the website and the designation of the movie as a TAS.

Logos are subject to approval by [Staff|the senior publisher].

Some examples of currently-in-use logos can be seen at [Encoders/Logos].

For a logo creation guide, see [Encoding Guide/Logo].

! Subtitles

The following information must be present in the encode, normally in the form of
hard-coded (i.e. embedded in the video stream directly) subtitles:

# What game is being played in the movie
# Branch, if applicable
# Who created (played) the movie
# The length of the movie, directly derived from the number of frames
# The rerecord count
# A pointer to this website (__[=|https://TASVideos.org/]__)
#* Please be careful to not make typing errors in the address. A simple mistake to happen is adding ''www'' into the address, don't make this mistake.
# A mention that the movie is tool-assisted.

The total duration of the subtitles should be approximately 10 seconds, divided into two parts of five seconds as follows:

The first part:
 ''Title''
 '''branch''' (if applicable)
 Played by ''PlayerName''
 Playing time:  ''hh:mm:ss.ss''
 Rerecord count: ''number''
The second part:
 This is a tool-assisted recording.
 For details, visit [=|https://TASVideos.org/]

The first part discourages people from claiming the movie their own work, and also prepares the viewer for what will be shown.%%%
The second part explains that the video is not a real-time playing performance. It is explained briefly, and a pointer to the [FrontPage|TASVideos website] is given to provide the opportunity to read the full details, as well as to point to a repository of more videos.

The subtitles should appear within the movie once gameplay has started, while not blocking any relevant parts of gameplay. They should also not be visible during fade transitions, including fade-ins and -outs. The rationale for this is to ensure that full segments of gameplay ripped from an encode will still show the information described above. Therefore, subtitles should appear after any intro scenes.

If having the subtitles in 2 parts cover important parts of gameplay no matter what, the subtitles can be split in three (with a split in the first subtitle after the line mentioning the TAS author) or even four parts (with a split between the first and second lines of the second part). Further linebreaks can be added as one sees fit.

If a run has multiple fully independent segments, there should be subtitles near the beginning of __each of them__. As an example, Super Metroid which has Ceres Station and Planet Zebes segments, subtitles should appear close to the start of both of those. This only applies to games which feature an often ignored intro level or similar. In order to determine the need for additional subtitles, use your judgment to decide if realistically people who have only seen the latter segment(s) of a game would believe they've seen all the necessary gameplay.

If the author of the TAS has a specific request for encoding their movie (e.g. including RTA time in the subtitles), they should generally be granted, with the exception of special logos, as they're an identifier on who encoded the movie rather than who made the TAS. If unsure if the request should be granted or not, ask a Senior Publisher.

For very long runs, it is also advisable to add more subtitles throughout.
----
!! Container format

We currently prefer the MP4 container format as it is a highly compatible container that can be played on all web browsers and devices.
----
!! Video codec

Our encodes presently use [https://www.videolan.org/developers/x264.html|x264] for video compression, for being the best currently available compromise between:
# Preserving all interesting video information (color, brightness, animation, movement), and
# Having as small a file size as possible.

Other codecs may be permissible, so long as they are freely available for all operating systems; it is strongly recommended that you consult a [Staff|publisher or admin] before doing so.

It is highly recommended that you experiment with different settings when encoding; a great deal of settings impact file size and visual quality.  In particular, ''bit rate is not the only thing that affects the visual quality!''

See [=EncoderGuidelines.html#Tips|tips] below.
----
!! Audio codec

The preferred codec of choice for this website is AAC as all browsers and devices can play it without assistance. The MP4 container also supports the Opus codec, but it lacks support.

For AAC, use the Fraunhofer FDK AAC encoder (requires a custom build of FFmpeg that you have to make yourself) or [https://www.videohelp.com/software/Nero-AAC-Codec|Nero's AAC encoder]. With either of them, set to the HE-AAC profile (keeps the quality at acceptable levels at low bitrates) and use a VBR setting that corresponds to a bitrate range of 32-64 kbps.

For Opus, use --bitrate to adjust quality. Reasonable initial values are about 40-80 kbps, depending on the system and audio complexity of the game. There are no other settings worth tweaking.

For CD-based games using [http://en.wikipedia.org/wiki/Red_Book_%28audio_CD_standard%29|Red Book] audio:
* Bitrate may be additionally raised to preserve quality.
* Use of a dump of the game that preserves the audio tracks losslessly (normally in .bin/.cue format) is required.

Furthermore, you may (and should) adjust the bitrates as needed, depending on the properties of the sound in the particular game.
----
!! Most common reasons of not accepting an encoded video file for publication on this site

* Video has incorrect resolution or aspect ratio information.
* Quality is below that of current publications (where visual quality is roughly equal, smaller encodes are preferred).
* Bad audio-video sync or drift (e.g. audio lags incrementally behind the video).
* Poorly placed or incomplete subtitles (overlaps action, does not contain all relevant information such as site information, etc.)
* Unacceptable logo (does not contain site information or distracts unnecessarily from said information, or is inappropriate or too long).
----
!! Tips

! x264

The following tips are for x264, which the site normally uses for video encoding.

* Constant rate factor 20 (--crf 20) is considered the site standard for video quality; it is recommended unless this provides an unacceptably large bitrate.
** Good average bitrates vary between 140-250 kbit/s for NES, SMS or Game Boy games and 200-600 kbit/s for other consoles, depending on the video content.  Some Genesis games and most N64 and PSX games may exceed this.
** Using CRF 16 is really recommended in 3D N64 games, since at CRF 20, the encode gets a lot of artifacts and it could be unappealing to the viewers.
** If you are aiming for a specific bit-rate, use multi-pass encoding. It nearly always improves the quality-to-size ratio.
* Set the x264 preset to "veryslow". This increases the quality of the video at the expense of encoding speed.
* Increase the keyint value to improve encoding efficiency and quality. The default is 250; values higher than 600 are discouraged. Do not set the keyint value too high, as this reduces the ability to seek in the video and inconveniences viewers. Most published encodes use either 600 or 300.
* Do not use "noise reduction". Emulators do not simulate antenna or tape signal degradation, so there's no point in trying to reduce it.

For some data on tuning x264's more relevant settings, see [=forum/topics/9559|this thread].

! Antialiasing (optional)

For polygon-based platforms (N64 outside of AngryLion, Dolphin), the following will help improve visual quality:
* Use the maximum anti-aliasing setting.
* Use the maximum full-screen anti-aliasing setting (FSAA).
* Use the maximum anisotropic filtering setting.
* Ensure that in your display card settings, there is no color-correction/skew curve active.
* Make 2 AVI dumps, one at native res for the downloadable encode and one at a high resolution (usually 4K res, or 2160p) for the YouTube encode.

For more info and examples on antialiasing settings, see [=forum/topics/6024|this thread].

!! Per-platform screenshot size

* The screenshot size limit is 45,000 bytes. If PNG can't be gotten below this, use JPEG.
* The correct screenshot resolution may not be among ones listed (especially for Arcade, Wii, OS).
** Admin-level users can override this check.

!! Checking your encode.

After you've made an encode, please ensure it encoded properly, and meets the above criteria. Go over the [EncoderGuidelines/EncodeChecklist|checklist].

!! See also
* [Encoding Guide]
* [=/Forum/Subforum/52|Encoder's Corner forum]
""";
}
