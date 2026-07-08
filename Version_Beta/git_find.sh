#!/bin/bash

# ./git_find.sh repo path search tmpfile
# repo папка проекта GIT, например E:\Projects\dev_promed_pg
# path путь внутри проекта GIT относительно корня (слеш как в линукс), например . или ./dbo
# search искомая строка
# tmpfile временный файл

# разбор параметров
ARG_REPO=$1
ARG_PATH=$2
ARG_SEARCH=$3
ARG_TMPFILE=$4

function script_main () {(
  cd "${1:-.}"

  find $2 -iname "*$3*"
)}

script_main "$ARG_REPO" "$ARG_PATH" "$ARG_SEARCH" | tee -a $ARG_TMPFILE
