@echo off
csc -nologo source\*.cs properties\*.cs -out:which.exe
copy /y which.exe \usr\local\bin 
