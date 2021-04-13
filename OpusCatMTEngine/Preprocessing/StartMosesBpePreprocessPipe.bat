@ set sourcelang=%1
@ set tcmodel=%2
@ set bpemodel=%3
@ chcp 65001 > nul
@ title OPUS-CAT MT engine - %1
@ Preprocessing\mosesprocessor.exe --stage preprocess --sourcelang %sourcelang% --tcmodel %tcmodel% | Preprocessing\apply_bpe.exe -c %bpemodel%