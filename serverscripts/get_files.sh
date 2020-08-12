#!/bin/bash

# Read in the temporary environment variables.
source ~/tempconfigfile

DATE=`date`
echo \[$DATE\] Downloading files. >> $DESTINATION_PATH/get_files.log
# LFTP to the $MAIN_SITE, grabbing $MEDIA_FILE and $MYSQL_FILE and putting them in $DESTINATION_PATH with their names.
lftp --env-password $MAIN_SITE -p $MAIN_SITE_PORT -e "set ftp:use-feat false; set ssl-allow false; user $LFTP_USERNAME $LFTP_PASSWORD; pget -O $DESTINATION_PATH $MEDIA_FILE $MYSQL_FILE; bye"

DATE=`date`
echo \[$DATE\] Importing SQL file. >> $DESTINATION_PATH/get_files.log
# Import the SQL file
cd $DESTINATION_PATH
gunzip -d $MYSQL_FILE
mysql -p$MYSQL_PASSWORD -e "SET autocommit=0; SOURCE $DESTINATION_PATH/$UNGZ_MYSQL_FILE; COMMIT;"

DATE=`date`
echo \[$DATE\] Updating media directory. >> $DESTINATION_PATH/get_files.log
# Put the media files in the appropriate place.
unzip -o -j $DESTINATION_PATH/$MEDIA_FILE -d $MEDIA_DIRECTORY

rm $DESTINATION_PATH/$MEDIA_FILE
rm $DESTINATION_PATH/$UNGZ_MYSQL_FILE

DATE=`date`
echo \[$DATE\] Operation finished. >> $DESTINATION_PATH/get_files.log