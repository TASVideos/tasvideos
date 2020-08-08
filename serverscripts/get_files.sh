#!/bin/bash

# Read in the temporary environment variables.
source ~/tempconfigfile

# LFTP to the $MAIN_SITE, grabbing $MEDIA_FILE and $MYSQL_FILE and putting them in $DESTINATION_PATH with their names.
lftp --env-password $MAIN_SITE -p $MAIN_SITE_PORT -e "set ftp:use-feat false; set ssl-allow false; user $LFTP_USERNAME $LFTP_PASSWORD; pget -O $DESTINATION_PATH $MEDIA_FILE $MYSQL_FILE; bye"

# Import the SQL file.

# Put the media files in the appropriate place.
unzip -u $DESTINATION_PATH/$MEDIA_FILE -d $MEDIA_DIRECTORY

rm $DESTINATION_PATH/$MEDIA_FILE
# rm $DESTINATION_PATH/$MYSQL_FILE
