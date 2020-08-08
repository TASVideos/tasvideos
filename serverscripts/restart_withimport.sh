#!/bin/bash
source ~/homedir

echo "restart_withimport script started" >> $HOME_DIR/cronlog.txt
kill $(ps aux | grep 'TASVideos.dll' | awk '{print $2}')
$HOME_DIR/tasvideos/serverscripts/pull.sh >> $HOME_DIR/cronlog.txt
$HOME_DIR/tasvideos/serverscripts/start_withimport.sh

