@ set modeldir=%1
@ chcp 65001 > nul
@ title OPUS-CAT MT engine - %1
@ Preprocessing\process.exe --stage preprocess --sourcelang sv --tcmodel %modeldir%\source.tcmodel | Preprocessing\apply_bpe.exe -c %modeldir%\source.bpe | Marian\marian.exe decode --log-level=warn -c %modeldir%\decoder.yml | Preprocessing\process.exe --stage postprocess --targetlang fi