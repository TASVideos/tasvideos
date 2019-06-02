echo "starting restart with import" >> /home/tasvideos/cronlog.txt
date >> /home/tasvideos/cronlog.txt
screen -X -S tasvideos quit
screen -dmS tasvideos /home/tasvideos/tasvideos/restart_withimport.sh
echo "done with restart with import script" >> /home/tasvideos/cronlog.txt
date >> /home/tasvideos/cronlog.txt
