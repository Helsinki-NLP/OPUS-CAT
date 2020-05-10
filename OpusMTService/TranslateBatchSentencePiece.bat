@ set modeldir=%1
@ chcp 65001 > nul
@ title Fiskmo MT engine - %1
@ marian-decoder.exe -i %2 -o %3 --log-level=warn -c %modeldir%\decoder.yml