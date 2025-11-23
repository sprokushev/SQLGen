#!/bin/bash

# ./git-nomerged-ver.sh repo-path ver-prefix tmpfile
# repo-path папка проекта GIT
# ver-prefix префикс версии
# tmpfile временный файл

# разбор параметров
ARG_REPO_PATH=$1
ARG_VER_PREFIX=$2
ARG_TMPFILE=$3

function branch_merged () {
  local SRC="$1"
  local DEST="${2:-master}"
  local BASE=$(git merge-base "$DEST" "$SRC")
  local SRC_HASH=$(git rev-parse "$SRC")
  [[ "$BASE" == "$SRC_HASH" ]]
}

function script_main () {(
  cd "${1:-.}"

  local VER_PREFIX PREV_VER
  VER_PREFIX=$2

  for VER in $(git branch -r --no-merged=master --format="%(refname:short)" --list "origin/$VER_PREFIX".* | sort --version-sort | tail -25 )
  do
    VER="${VER#origin/}"

    VER_YAML=$(git ls-tree --name-only "$VER" version/ | grep "${VER}_" | grep -E "\.yml$")

    if [[ -z "$VER_YAML" ]]
    then
      echo $VER "YAML not found!"
    else
      if ! git show "$VER:$VER_YAML" | grep -q "#NOCUMULATIVE"
      then
        if [[ -n "$PREV_VER" ]] && ! branch_merged "$PREV_VER" "$VER" ; then
          echo "$VER (not contain $PREV_VER)"
        else
          echo "$VER"
        fi
        PREV_VER="$VER"
      fi
    fi
  done
)}

rm -f "$ARG_TMPFILE"
script_main "$ARG_REPO_PATH" "$ARG_VER_PREFIX" | tee -a $ARG_TMPFILE
