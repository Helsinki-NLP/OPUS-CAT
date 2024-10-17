#!/bin/sh

# Specify the directory in which the script is located
script_dir="$(cd "$(dirname "$0")" && pwd)"

# Change to the OpusCatMtEngine directory
opuscatdir="$script_dir/OpusCatMtEngine"
cd "$opuscatdir"

# Set the relative path for DYLD_LIBRARY_PATH based on the script directory
export DYLD_LIBRARY_PATH="$opuscatdir/python3-macos-3.8.13-universal2/lib"

# Set PYTHONHOME and run OpusCatMtEngineCore
PYTHONHOME="$opuscatdir/python3-macos-3.8.13-universal2" "$opuscatdir/OpusCatMtEngineCore“
