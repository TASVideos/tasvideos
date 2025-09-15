#!/bin/bash
### BEGIN INIT INFO
# Provides:             tasvideos
# Required-Start:       nginx postgresql redis-server
# Required-Stop:        nginx postgresql redis-server
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
  echo 'Start()'

  stop

  if [ -f \"$ENVIRONMENT_FILE\" ]; then
    ENV=$(cat \"$ENVIRONMENT_FILE\")
  else
    ENV=Production
  fi

  echo 'Starting TASVideos website with' "$ENV" 'profile.'

  su -c "start-stop-daemon -SbmCv -p \"$PIDFILE_TEMP\" -d \"$ACTIVE_DIRECTORY\" -x \"./TASVideos\" -- --urls \"http://127.0.0.1:5000\" --environment $ENV --StartupStrategy Migrate -c Release" $ACTIVE_USER

  cp $PIDFILE_TEMP $PIDFILE
  chown root:root $PIDFILE

  echo 'Website started.'
}

# Stop the TASVideos website.
stop() {
  echo 'Stop()'

  if [ ! -f "$PIDFILE" ] || ! kill -0 "$(cat "$PIDFILE")"; then
    echo 'Website not running'
  else
    echo 'Stopping website...'

    su -c "start-stop-daemon -K -p \"$PIDFILE\" -u \"$ACTIVE_USER\" --retry 5"
    rm -f "$PIDFILE"

    echo 'Website stopped.'
  fi
}

# Grab code from Git and publish (compile) it.
build() {
  echo 'Build()'

  su -c "cd \"$GIT_PULL_LOCATION\" && git fetch --tags --force && git pull && dotnet publish TASVideos/TASVideos.csproj -c Release -o \"$BUILD_DIRECTORY\"" $ACTIVE_USER
}

restart() {
  echo 'Restart()'
  
  stop
  start
}

# Move files from the live site directory to a temp directory.
# Move the published files into the live site directory.
deploy() {
  echo 'Starting deployment.'

  # mv current website into temp directory
  mv $ACTIVE_DIRECTORY $TEMP_DIRECTORY

  # mv build directory into build location
  mv $BUILD_DIRECTORY $ACTIVE_DIRECTORY

  # recreate symlinks
  ln -s $MEDIA_SYMLINK_DIRECTORY $ACTIVE_MEDIA_LOCATION
  ln -s $SAMPLEDATA_SYMLINK_DIRECTORY $ACTIVE_SAMPLEDATA_LOCATION

  echo 'Deploy complete.'
}

# Move files from the live site directory into a different temp directory.
# Move the previous temp directory into the live site directory.
undeploy() {
  echo 'Reverting last deployment.'

  ls $TEMP_DIRECTORY
  LS=$?

  if [ $LS -ne 0 ]; then
    echo 'Error received checking existence of previous deployment.  Aborting revert.'
    exit $LS
  fi

  # remove the current build
  rm -rf $ACTIVE_DIRECTORY

  # mv temp location into build location
  mv $TEMP_DIRECTORY $ACTIVE_DIRECTORY

  # recreate symlinks
  ln -s $MEDIA_SYMLINK_DIRECTORY $ACTIVE_MEDIA_LOCATION
  ln -s $SAMPLEDATA_SYMLINK_DIRECTORY $ACTIVE_SAMPLEDATA_LOCATION

  # delete secondary temp location

  echo 'Revert complete.'
}


# Delete the temp directory.
cleanup() {
  rm -rf $TEMP_DIRECTORY
}

case "$1" in
  start)
    start
    ;;
  stop)
    stop
    ;;
  restart)
    restart
    ;;
  reload)
    cleanup
    build
    stop
    deploy
    start
    ;;
  revert)
    stop
    undeploy
    restart
    ;;
  build-only)
    build
    ;;
  commands)
    echo start - Start the website without updating
    echo stop - Stop the website
    echo restart - Stop and Start the website without updating
    echo reload - Full update.  Pulls latest code and builds it.  Should restart the service afterwards. \(Recommended\)
    echo revert - Stop the website, revert to the previous build \(which was likely good\), and then start the website again.
    echo build-only - Pulls the latest code and compiles without affecting the state of the website
    ;;
  *)
    echo "Usage: $0 {start|stop|restart|reload|revert|build-only|commands}"
esac

EC=$?

if [ $EC -ne 0 ]; then
  echo 'Error code' $EC 'received during' "$1" '. Some step failed.'
fi

export runlevel=$TMP_SAVE_runlevel_VAR
