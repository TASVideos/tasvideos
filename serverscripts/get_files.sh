#!/bin/bash

# Read in the temporary environment variables.
source ~/tempconfigfile

# LFTP to the $MAIN_SITE, grabbing $MEDIA_FILE and $MYSQL_FILE and putting them in $DESTINATION_PATH with their names.
lftp --env-password $MAIN_SITE -e "pget -O $DESTINATION_PATH $MEDIA_FILE -O $DESTINATION_PATH $MYSQL_FILE; bye"

# Import the SQL file.

# Put the media files in the appropriate place.

