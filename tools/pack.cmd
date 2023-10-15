@echo off

REM    SpecProbe  Copyright (C) 2023  Aptivi
REM
REM    This file is part of SpecProbe
REM
REM    SpecProbe is free software: you can redistribute it and/or modify
REM    it under the terms of the GNU General Public License as published by
REM    the Free Software Foundation, either version 3 of the License, or
REM    (at your option) any later version.
REM
REM    SpecProbe is distributed in the hope that it will be useful,
REM    but WITHOUT ANY WARRANTY; without even the implied warranty of
REM    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
REM    GNU General Public License for more details.
REM
REM    You should have received a copy of the GNU General Public License
REM    along with this program.  If not, see <https://www.gnu.org/licenses/>.

for /f "tokens=* USEBACKQ" %%f in (`type version`) do set version=%%f
set releaseconfig=%1
if "%releaseconfig%" == "" set releaseconfig=Release

:packbin
echo Packing binary...
"%ProgramFiles%\WinRAR\rar.exe" a -ep1 -r -m5 %temp%/%version%-bin.rar "..\SpecProbe.Bin\net6.0\"
"%ProgramFiles%\WinRAR\rar.exe" a -ep1 -r -m5 %temp%/%version%-demo.rar "..\SpecProbe.ConsoleTest.Bin\net6.0\"
if %errorlevel% == 0 goto :complete
echo There was an error trying to pack binary (%errorlevel%).
goto :finished

:complete
move %temp%\%version%-bin.rar
move %temp%\%version%-demo.rar

echo Pack successful.
:finished
