@echo off

REM switch to script dir, should be custom model dir
REM cd /D "%~dp0"

REM setlocal EnableExtensions DisableDelayedExpansion

:parseref
set ref=%ref% %~1
shift
set arg=%1
REM avoid infinite loop
if /i "%arg%"=="" goto end
if /i not "%arg:~0,3%"=="OOD" goto parseref
set ref=%ref:~1%

set oodsize=%arg:~3%
shift

:parsehyp
set hyp=%hyp% %~1
shift
set arg=%1
if /i not "%arg%"=="" goto parsehyp
set hyp=%hyp:~1%


set "split1_1=%hyp%_0.txt"
set "split1_2=%hyp%_1.txt"
set "split2_1=%ref%_0.txt"
set "split2_2=%ref%_1.txt"
set "score1=%hyp%_0.score.txt"
set "score2=%hyp%_1.score.txt"


if exist "%split1_1%" (
	del "%split1_1%"
)
if exist "%split1_2%" (
	del "%split1_2%"
)
if exist "%split2_1%" (
	del "%split2_1%"
)
if exist "%split2_2%" (
	del "%split2_2%"
)

REM the argument order from Marian is validation target, size of OOD set, model output
CALL :Split "%hyp%" , %oodsize%
CALL :Split "%ref%" , %oodsize%



Evaluation\sacrebleu_wrapper.exe --score-only --input "%split1_1%" "%split2_1%" 2> nul > "%score1%"
Evaluation\sacrebleu_wrapper.exe --score-only --input "%split1_2%" "%split2_2%" 2> nul > "%score2%"


set /p scorevalue1= < "%score1%"
set /p scorevalue2= < "%score2%"
set scorevalue1=%scorevalue1:.=%
set scorevalue2=%scorevalue2:.=%


set /a combinedscore=%scorevalue1%+%scorevalue2%

echo %combinedscore%

:end
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
		REM Convert sentencepiece space marker into actual space
        set "linecontent=!linecontent:▁= !"
		REM The space after colon removes the leading space left after sentencepiece detok
		for %%b in (!file!) do (   
            >>"%~1_%%b.txt" echo(!linecontent:*:=!
        )
    )

    endlocal

