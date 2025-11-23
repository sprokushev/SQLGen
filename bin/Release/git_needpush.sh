#!/bin/bash

# утилита git-needpush.sh repo 
# repo папка проекта GIT, например E:\Projects\dev_promed_pg

# разбор параметров
ARG_REPO=$1

# переключаемся на папку проекта
cd "${1:-.}"

set -o pipefail
source "$ARG_REPO/utility/functions.sh" || exit 1

# проверяем на git-репозиторий
check_git_repo

TMP_DIR="$ARG_REPO/utility/gpa/temp"
dir_must_exist "$TMP_DIR"

function precache_unsync_branches {(
  set -e

  # подготовка списка несинхронизированных локальных веток для дальнейшей обработки
  local FMT='%(refname:short)::%(upstream:trackshort)::%(upstream:short)::%(upstream:track)'
  git for-each-ref --format="$FMT" refs/heads \
    | awk -F'::' '$2 != "="' > "$TMP_DIR/branches.unsync"

  # устаревшие ветки: ветка слежения была удалена
  #awk -F'::' '$4=="[gone]" {print $1}' "$TMP_DIR/branches.unsync" > "$TMP_DIR/branches.obsolete"

  # отставшие ветки: ветку можно обновить перемоткой
  #awk -F'::' '$2=="<" {print $3 ":" $1}' "$TMP_DIR/branches.unsync" > "$TMP_DIR/branches.behind"

  # далее случаи, по которым не разобраться автоматически - по ним выдаются ошибки

  # конфликт имён: имена локальной и ремоут-ветки совпадают, но отслеживание не настроено
  #awk -F'::' '$2=="" {print $1}' "$TMP_DIR/branches.unsync" | sort > "$TMP_DIR/branches.untracked"
  #comm -12 "$TMP_DIR/branches.remote" "$TMP_DIR/branches.untracked" > "$TMP_DIR/branches.namesake"

  # история разошлась
  # dev исключаем, т.к. история по ней будет расходиться часто. про pull/push и так все знают
  #awk -F'::' '$2=="<>" && $1!="dev" {print $1 "::" $3 "::" $4}' "$TMP_DIR/branches.unsync" > "$TMP_DIR/branches.diverged"

  # ветки не запушены, исключая ветки из "конфликта имён", которые удалось починить
  echo -n > "$TMP_DIR/branches.ahead.ignore"
  [[ -n "$1" ]] && echo "$1" >> "$TMP_DIR/branches.ahead.ignore"
  awk -F'::' '($2==">" || $2=="") {print $1}' "$TMP_DIR/branches.unsync" > "$TMP_DIR/branches.ahead"
)}

function gpa_check_ahead_branches {(
  set -e
  echo ''
  echo 'Список незапушеных веток:'
  comm -23 <(sort "$1") <(sort "$2") | sed "s/^.*$/  '&'/"
  echo 'Конец списка'
)}

# запускаем весь скрипт в subshell, чтобы отловить exitcode
script_main () {(

  # subshell будет прерван по первой ошибке
  set -e -o pipefail

  # определяем главную ветку
  local MASTER_BRANCH="master"
  local REMOTE_REPO="origin"

  echo "Путь запуска: '$(pwd)'"

  precache_unsync_branches ""

  gpa_check_ahead_branches "$TMP_DIR/branches.ahead" "$TMP_DIR/branches.ahead.ignore"

  echo 'Нажмите Enter'
  read
)}

script_main 2>&1 
