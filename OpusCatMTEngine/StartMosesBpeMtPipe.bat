@ set modeldir=%1
@ set sourcelang=%2
@ chcp 65001 > nul
@ title OPUS-CAT MT engine - %1
@ Preprocessing\mosesprocessor.exe --stage preprocess --sourcelang %sourcelang% --tcmodel %modeldir%\source.tcmodel | Preprocessing\apply_bpe.exe -c %modeldir%\source.bpe | Marian\marian.exe decode --log-level=warn -c %modeldir%\decoder.yml | Preprocessing\process.exe --stage postprocess --targetlang fi