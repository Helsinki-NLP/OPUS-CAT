@echo off

REM switch to script dir, should be custom model dir
REM cd /D "%~dp0"

REM setlocal EnableExtensions DisableDelayedExpansion
set "split1_1=%3_0.txt"
set "split1_2=%3_1.txt"
set "split2_1=%1_0.txt"
set "split2_2=%1_1.txt"
set "score1=%3_0.score.txt"
set "score2=%3_1.score.txt"

if exist %split1_1% (
	del %split1_1%
)
if exist %split1_2% (
	del %split1_2%
)
if exist %split2_1% (
	del %split2_1%
)
if exist %split2_2% (
	del %split2_2%
)

REM the argument order from Marian is validation target, size of OOD set, model output
CALL :Split %3 , %2
CALL :Split %1 , %2

Evaluation\sacrebleu_wrapper.exe --score-only --input %split1_1% %split2_1% 2> nul > %score1%
Evaluation\sacrebleu_wrapper.exe --score-only --input %split1_2% %split2_2% 2> nul > %score2%

set /p scorevalue1= < %score1%
set /p scorevalue2= < %score2%
set scorevalue1=%scorevalue1:.=%
set scorevalue2=%scorevalue2:.=%

set /a combinedscore=%scorevalue1%+%scorevalue2%

echo %combinedscore%

EXIT /B %ERRORLEVEL%

:Split
	setlocal enabledelayedexpansion

    set "nLines=%~2"
    set "line=0"
	
	REM the findstr command appends a number to each line, the string replacement on the last line
	REM strips it. this ensures that blank lines aren't discarded (if they are, parallel files go out of sync)
    for /f "usebackq delims=" %%a in (`findstr /N "^" "%~1"`) do (
        set /a file=!line!/%nLines%
		if !line! LEQ !nLines! (set /a line+=1)
		set "linecontent=%%a"
		REM Remove spaces between sentencepiece tokens
		set "linecontent=!linecontent: =!"
		REM Conver sentencepiece space marker into actual space
        set "linecontent=!linecontent:▁= !"
		REM The space after colon removes the leading space left after sentencepiece detok
		for %%b in (!file!) do (   
            >>"%~1_%%b.txt" echo(!linecontent:*: =!
        )
    )

    endlocal

