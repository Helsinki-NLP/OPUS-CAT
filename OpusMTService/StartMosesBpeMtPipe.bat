@ set modeldir=%1
@ chcp 65001 > nul
@ title Fiskmo MT engine - %1
@ process.exe --stage preprocess --sourcelang sv --tcmodel %modeldir%\source.tcmodel | apply_bpe.exe -c %modeldir%\source.bpe | Marian\marian.exe decode -c %modeldir%\decoder.yml | process.exe --stage postprocess --targetlang fi