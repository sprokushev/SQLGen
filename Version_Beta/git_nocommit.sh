#!/bin/bash

# ./git_nocommit.sh repo
# repo папка проекта GIT, например E:\Projects\dev_promed_pg
# tmpfile временный файл

# разбор параметров
ARG_REPO=$1
ARG_TMPFILE=$2

function script_main () {(
  cd "${1:-.}"

  STAGED_MODIFIED_FILES=$(git status -s)
  #echo "$STAGED_MODIFIED_FILES"

  if [[ -n "$STAGED_MODIFIED_FILES" ]]; then
    echo "NEEDCOMMIT"
    exit 10006
  else
    echo "OK"
    exit 0
  fi
)}

script_main "$ARG_REPO" | tee -a $ARG_TMPFILE
