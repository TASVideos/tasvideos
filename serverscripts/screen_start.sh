#!/bin/bash
source ~/homedir

screen -dmS tasvideos $HOME_DIR/tasvideos/serverscripts/start.sh
echo "done with restart script" >> $HOME_DIR/cronlog.txt
date >> $HOME_DIR/cronlog.txt