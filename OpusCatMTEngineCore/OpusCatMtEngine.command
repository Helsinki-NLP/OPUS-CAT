#!/bin/sh
opuscatdir=$(dirname $0)/OpusCatMtEngine
cd $opuscatdir
export DYLD_LIBRARY_PATH=$(realpath ./python3-macos-3.8.13-universal2/lib)
PYTHONHOME=./python3-macos-3.8.13-universal2 ./OpusCatMtEngineCore
