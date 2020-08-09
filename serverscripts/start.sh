#!/bin/bash
source ~/homedir

echo "starting dot net" >> $HOME_DIR/cronlog.txt
dotnet run --project "$HOME_DIR/tasvideos/TASVideos" --urls "http://127.0.0.1:5000" --environment "Demo" --StartupStrategy Minimal

