@ set modeldir=%1
@ chcp 65001 > nul
@ title Fiskmo MT engine - %1
@ Preprocessing\spm_encode.exe --model %modeldir%\source.spm | Marian\marian.exe decode --log-level=warn -c %modeldir%\decoder.yml --max-length=200 --max-length-crop