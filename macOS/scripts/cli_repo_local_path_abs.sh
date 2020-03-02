#!/bin/bash

# Script prints absolute path to local  repository of IVPN CLI sources
# It reads relative path info from 'config/cli_repo_local_path.txt'
# How to use in subscripts:
#   CLI_REPO_ABS_PATH=$("./cli_repo_local_path_abs.sh")

# Exit immediately if a command exits with a non-zero status.
set -e

cd "$(dirname "$0")"
cd "config"
RELATIVE_PATH=$(<'cli_repo_local_path.txt')
cd $RELATIVE_PATH

pwd
