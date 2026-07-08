#!/bin/bash

# ./git-lastcommit.sh repo-path task tmpfile
# repo-path папка проекта GIT
# task ветка задачи
# tmpfile временный файл

# разбор параметров
ARG_REPO_PATH=$1
ARG_TASK=$2
ARG_TMPFILE=$3

function script_main () {(
  cd "${1:-.}"

  local TASK
  TASK=$2

  for AGE in $(git show -s --format=%ad --date=short "origin/$TASK")
  do
    echo $AGE
    break
  done
)}

function pause(){
   read -p "$*"
}

rm -f "$ARG_TMPFILE"
# pause 'Press [Enter] key to continue...'
script_main "$ARG_REPO_PATH" "$ARG_TASK" | tee -a $ARG_TMPFILE
# pause 'Press [Enter] key to continue...'
