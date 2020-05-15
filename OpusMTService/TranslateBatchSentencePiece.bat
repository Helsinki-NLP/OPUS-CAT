@ set modeldir=%1
@ chcp 65001 > nul
@ title Fiskmo MT engine - %1
@ marian.exe decode -i %2 -o %3 --log-level=info -c %modeldir%\batch.yml