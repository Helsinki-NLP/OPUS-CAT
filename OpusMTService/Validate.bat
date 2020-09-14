@echo off

REM switch to script dir, should be custom model dir
cd /D "%~dp0"

REM setlocal EnableExtensions DisableDelayedExpansion
set "split1_1=%3_0.txt"
set "split1_2=%3_1.txt"
set "split2_1=%1_0.txt"
set "split2_2=%1_1.txt"
del %split1_1%
del %split1_2%
del %split2_1%
del %split2_2%


REM the argument order from Marian is validation target, size of OOD set, model output
CALL :Split %3 , %2
CALL :Split %1 , %2

Evaluation\sacrebleu_wrapper.exe --score-only --input %split1_1% %split2_1% 2> nul > score.1
Evaluation\sacrebleu_wrapper.exe --score-only --input %split1_2% %split2_2% 2> nul > score.2

set /p score1= < score.1
set /p score2= < score.2
set score1=%score1:.=%
set score2=%score2:.=%

set /a combinedscore=%score1%+%score2%

echo %combinedscore%

EXIT /B %ERRORLEVEL%

:Split
	setlocal enabledelayedexpansion

    set "nLines=%~2"
    set "line=0"

    for /f "usebackq delims=" %%a in (%~1) do (
        set /a file=!line!/%nLines%
		if !line! LEQ !nLines! (set /a line+=1)
		
        for %%b in (!file!) do (   
            >>"%~1_%%b.txt" echo(%%a
        )
    )

    endlocal

