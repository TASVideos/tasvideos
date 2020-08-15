#!/bin/bash
source ~/homedir

cd $HOME_DIR
git fetch --tags --force
git pull
