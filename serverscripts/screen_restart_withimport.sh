#!/bin/bash
source ~/homedir

echo "starting restart with import" >> $HOME_DIR/cronlog.txt
date >> $HOME_DIR/cronlog.txt
screen -X -S tasvideos quit
screen -dmS tasvideos $HOME_DIR/tasvideos/serverscripts/restart_withimport.sh
echo "done with restart with import script" >> $HOME_DIR/cronlog.txt
date >> $HOME_DIR/cronlog.txt
