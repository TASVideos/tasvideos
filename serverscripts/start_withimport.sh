#!/bin/bash
source ~/homedir

echo "starting dot net with import" >> $HOME_DIR/cronlog.txt
dotnet run --project "$HOME_DIR/tasvideos/TASVideos" --urls "http://0.0.0.0:5000" --environment "Demo" --StartupStrategy Import
