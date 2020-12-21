@ set modeldir=%1
@ chcp 65001 > nul
@ title Fiskmo MT engine - %1
@ Marian\marian.exe decode -i %2 -o %3 --log-level=error -c %modeldir%\batch.yml --max-length=200 --max-length-crop