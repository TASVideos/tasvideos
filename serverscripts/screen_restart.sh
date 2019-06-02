echo "starting restart script" >> /home/tasvideos/cronlog.txt
date >> /home/tasvideos/cronlog.txt
screen -X -S tasvideos quit
screen -dmS tasvideos /home/tasvideos/tasvideos/restart.sh
echo "done with restart script" >> /home/tasvideos/cronlog.txt
date >> /home/tasvideos/cronlog.txt
