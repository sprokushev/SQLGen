#!/bin/bash

# ./git-merged-ver.sh repo-path ver-prefix task tmpfile
# repo-path папка проекта GIT
# ver-prefix префикс версии
# task ветка задачи
# tmpfile временный файл

# разбор параметров
ARG_REPO_PATH=$1
ARG_VER_PREFIX=$2
ARG_TASK=$3
ARG_TMPFILE=$4

function script_main () {(
  cd "${1:-.}"

  local VER_PREFIX
  VER_PREFIX=$2

  local TASK
  TASK=$3

  for VER in $(git branch -r --merged="origin/$TASK" --format="%(refname:short)" --list "origin/$VER_PREFIX".* | sort -Vr | head -20)
  do
    echo $VER
  done

  for VER in $(git ls-tree -r "origin/$TASK" --full-tree --name-only version | grep .yml | sort -Vr | head -20)
  do
    echo $VER
  done
)}

function pause(){
   read -p "$*"
}

rm -f "$ARG_TMPFILE"
# pause 'Press [Enter] key to continue...'
script_main "$ARG_REPO_PATH" "$ARG_VER_PREFIX" "$ARG_TASK" | tee -a $ARG_TMPFILE
# pause 'Press [Enter] key to continue...'
