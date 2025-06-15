@echo if you have an older version of pingo use -s9.
FOR %%i in (*.png) DO pingo.exe -lossless -strip "%%i"
