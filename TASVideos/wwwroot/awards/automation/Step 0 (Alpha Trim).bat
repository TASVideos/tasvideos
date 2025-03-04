@echo This one doesn't work in a "FOR" function, for a reason idk.
@echo magick convert input.png -channel alpha -trim output.png

magick mogrify -path output -format png -channel alpha -trim *