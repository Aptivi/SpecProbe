#!/bin/bash

#   SpecProbe  Copyright (C) 2023  Aptivi
# 
#   This file is part of SpecProbe
# 
#   SpecProbe is free software: you can redistribute it and/or modify
#   it under the terms of the GNU General Public License as published by
#   the Free Software Foundation, either version 3 of the License, or
#   (at your option) any later version.
# 
#   SpecProbe is distributed in the hope that it will be useful,
#   but WITHOUT ANY WARRANTY; without even the implied warranty of
#   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#   GNU General Public License for more details.
# 
#   You should have received a copy of the GNU General Public License
#   along with this program.  If not, see <https://www.gnu.org/licenses/>.

# This script builds. Use when you have dotnet installed.
releaseconf=$1
if [ -z $releaseconf ]; then
	releaseconf=Release
fi

# Check for dependencies
dotnetpath=`which dotnet`
if [ ! $? == 0 ]; then
	echo dotnet is not found.
	exit 1
fi

# Download packages
echo Downloading packages...
"$dotnetpath" restore "../SpecProbe.sln" -p:Configuration=$releaseconf
if [ ! $? == 0 ]; then
	echo Download failed.
	exit 1
fi

# Build SpecProbe
echo Building SpecProbe...
"$dotnetpath" build "../SpecProbe.sln" -p:Configuration=$releaseconf
if [ ! $? == 0 ]; then
	echo Build failed.
	exit 1
fi

# Inform success
echo Build successful.
exit 0
