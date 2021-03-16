@ set modeldir=%1
@ set targetlang=%2
@ chcp 65001 > nul
@ title OPUS-CAT MT engine - %1
@ Preprocessing\spm_encode.exe --model %modeldir%\source.spm | powershell -Command "$input | %%{'>>%targetlang%<< ' + $_}" | Marian\marian.exe decode --log-level=warn -c %modeldir%\decoder.yml --max-length=200 --max-length-crop