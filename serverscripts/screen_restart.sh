#!/bin/bash
source ~/homedir

echo "starting restart script" >> $HOME_DIR/cronlog.txt
date >> $HOME_DIR/cronlog.txt
screen -X -S tasvideos quit
screen -dmS tasvideos $HOME_DIR/tasvideos/serverscripts/restart.sh
echo "done with restart script" >> $HOME_DIR/cronlog.txt
date >> $HOME_DIR/cronlog.txt
