#!/bin/bash
echo "starting dot net" >> /home/tasvideos/cronlog.txt
dotnet run --project "/home/tasvideos/tasvideos/TASVideos" --urls "http://127.0.0.1:5000" --environment "Demo"

