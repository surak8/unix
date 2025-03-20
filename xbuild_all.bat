@ECHO OFF
SETLOCAL
CALL xbuild > %dpn0.log
IF ERRORLEVEL 5 ECHO ACK-5!
IF ERRORLEVEL 4 ECHO ACK-4!
IF ERRORLEVEL 3 ECHO ACK-3!
IF ERRORLEVEL 2 ECHO ACK-2!
IF ERRORLEVEL 1 ECHO ACK-1: %%F failed to build
XCOPY /S/B/Q/Y %~dp0bin\*.exe %SYSTEMDRIVE%\usr\local\bin
XCOPY /S/B/Q/Y %~dp0bin\*.exe.* %SYSTEMDRIVE%\usr\local\bin
XCOPY /S/B/Q/Y %~dp0bin\*.pdb %SYSTEMDRIVE%\usr\local\bin
XCOPY /S/B/Q/Y %~dp0bin\*.XML %SYSTEMDRIVE%\usr\local\bin
