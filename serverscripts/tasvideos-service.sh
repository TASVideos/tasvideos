#!/bin/bash
### BEGIN INIT INFO
# Provides:             tasvideos
# Required-Start:       mysql nginx postgresql redis-server
# Required-Stop:        mysql nginx postgresql redis-server
# Should-Start:         $local_fs $network
# Should-Stop:          $local_fs $network
# Default-Start:        3 4 5
# Default-Stop:         0 1 2 6
# Short-Description:    TASVideos website server
# Description:          TASVideos website server, .net and Kestrel, v2
### END INIT INFO

ACTIVE_USER=tasvideos
HOME_DIR=/home/tasvideos
ENVIRONMENT_FILE=/home/tasvideos/environment.txt

DOTNET_RUNTIME=/usr/bin/dotnet
GIT_PULL_LOCATION=$HOME_DIR/tasvideos

MEDIA_SYMLINK_DIRECTORY=$HOME_DIR/website/static-files/media
SAMPLEDATA_SYMLINK_DIRECTORY=$HOME_DIR/website/static-files/sample-data

ACTIVE_DIRECTORY=$HOME_DIR/website/running
ACTIVE_MEDIA_LOCATION=$ACTIVE_DIRECTORY/wwwroot/media
ACTIVE_SAMPLEDATA_LOCATION=$ACTIVE_DIRECTORY/wwwroot/sample-data
BUILD_DIRECTORY=$HOME_DIR/build_output
TEMP_DIRECTORY=$HOME_DIR/temp

PIDFILE=/var/run/tasvideos.pid
PIDFILE_TEMP=$HOME_DIR/tasvideos.pid

# fix issue with DNX exception in case of two env vars with the same name but different case
TMP_SAVE_runlevel_VAR=$runlevel
unset runlevel

# Start the TASVideos website.
start() {
  if [ -f $PIDFILE ] && kill -0 $(cat $PIDFILE); then
    echo 'Service already running or was not stopped correctly.' >&2
    return 1
  fi

  if [ -f $ENVIRONMENT_FILE ]; then
    ENV=`cat $ENVIRONMENT_FILE`
  else
    ENV=Staging
  fi

  echo 'Starting TASVideos website with' $ENV 'profile.' >&2

  su -c "start-stop-daemon -SbmCv -x /usr/bin/nohup -p \"$PIDFILE_TEMP\" -d \"$ACTIVE_DIRECTORY\" -- ./TASVideos --urls \"http://127.0.0.1:5000\" --environment \"$ENV\" --StartupStrategy \"Migrate\" -c \"Release\"" $ACTIVE_USER
  cp $PIDFILE_TEMP $PIDFILE
  chown root:root $PIDFILE

  echo 'Website started.' >&2
}

# Stop the TASVideos website.
stop() {
  if [ ! -f "$PIDFILE" ] || ! kill -0 $(cat "$PIDFILE"); then
    echo 'Website not running' >&2
    return 1
  fi

  echo 'Stopping website...' >&2

  su -c "start-stop-daemon -K -p \"$PIDFILE\"" $WWW_USER
  rm -f "$PIDFILE"
  echo 'Website stopped.' >&2
}

# Grab code from Git and publish (compile) it.
build() {
  su -c "cd $GIT_PULL_LOCATION && git fetch --tags --force && git pull && dotnet publish . -c Release -o $BUILD_DIRECTORY" $ACTIVE_USER
}

# Move files from the live site directory to a temp directory.
# Move the published files into the live site directory.
deploy() {
  # mv old code into temp location
  mv $ACTIVE_DIRECTORY $TEMP_DIRECTORY

  # mv build directory into build location
  mv $BUILD_DIRECTORY $ACTIVE_DIRECTORY

  # recreate symlinks
  ln -s $MEDIA_SYMLINK_DIRECTORY $ACTIVE_MEDIA_LOCATION
  ln -s $SAMPLEDATA_SYMLINK_DIRECTORY $ACTIVE_SAMPLEDATA_LOCATION
}

# Delete the temp directory.
cleanup() {
  rm -rf $TEMP_DIRECTORY
}

# Copy script files (.js and .css) into the live directories.
copy_scripts() {
  # TODO: Write this code.
  echo 'Script not done.'
}

case "$1" in
  start)
    start
    ;;
  stop)
    stop
    ;;
  restart)
    stop && start
    ;;
  build)
    build && stop && deploy && start && cleanup
    ;;
  update-scripts)
    copy_scripts
    ;;
  *)
    echo "Usage: $0 {build|restart|start|stop|update-scripts}"
esac

EC=$?

if [ $EC -ne 0 ]; then
  echo 'Error code' $EC 'received during' $1 '. Some step failed.'
fi

export runlevel=$TMP_SAVE_runlevel_VAR
