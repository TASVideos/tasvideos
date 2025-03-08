@echo This uses FFMPEG cause it's actually shockingly quick to set this up.

@echo There's a single file in the "template" folder which holds the podiums for both TAS(T) and TASer(P) (podiums.psd).
@echo Be sure to change the year to whatever the corresponding current year is and be sure to alpha trim as well before putting it through this script.

@echo This is the script that can automate *almost all* the award podiums, so it doesn't have to be adjusted manually.
@echo What this script doesn't do is place the year at the end, so you have to adjust for it.

@echo This does not automate the following: TAS GameBoy, TAS Homebrew, TAS Nintendo 3D, Both Exotic, TAS of, TASer of, Rookie of.
@echo "TAS of" is quick to adjust for as you just need to just copy paste the grey plate backing.
@echo "Exotic" all you need to do is just copy paste the lower quarter to replace.

@echo For TASer and Rookie refer to the masterfile, as that's set up for you (hopefully).

FOR %%a in (*.png) DO ffmpeg -n -hide_banner -i "%%~a" -i T2024.png -filter_complex "[0:v]crop=iw:ih-15:0:0[main]; [main][1:v]vstack" -pix_fmt rgb32 "output/%%~a"
FOR %%a in (*.png) DO ffmpeg -n -hide_banner -i "%%~a" -i P2024.png -filter_complex "[0:v]crop=iw:ih-17:0:0[main]; [main][1:v]vstack" -pix_fmt rgb32 "output/%%~a"