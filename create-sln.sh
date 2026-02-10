#!/usr/bin/env bash

UNITY_EXE="/d/Unity/6000.3.6f1/Editor/Unity.exe"
UNITY_DIR="/c/Users/Edvin/Roguelike PvE auto-battler"

"$UNITY_EXE" \
  -batchmode -quit \
  -projectPath "$UNITY_DIR" \
  -executeMethod Packages.Rider.Editor.RiderScriptEditor.SyncSolution
