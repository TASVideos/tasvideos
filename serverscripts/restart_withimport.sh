#!/bin/bash
echo "restart_withimport script started" >> /home/tasvideos/cronlog.txt
kill $(ps aux | grep 'TASVideos.dll' | awk '{print $2}')
/home/tasvideos/tasvideos/serverscripts/pull.sh >> /home/tasvideos/cronlog.txt
/home/tasvideos/tasvideos/serverscripts/start_withimport.sh

