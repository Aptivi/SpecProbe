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

# This script builds and packs the artifacts. Use when you have MSBuild installed.
version=$(cat version)
releaseconf=$1
if [ -z $releaseconf ]; then
	releaseconf=Release
fi

# Check for dependencies
rarpath=`which rar`
if [ ! $? == 0 ]; then
	echo rar is not found.
	exit 1
fi

# Pack binary
echo Packing binary...
"$rarpath" a -ep1 -r -m5 /tmp/$version-bin.rar "../SpecProbe.Bin/net6.0/"
"$rarpath" a -ep1 -r -m5 /tmp/$version-demo.rar "../SpecProbe.ConsoleTest.Bin/net6.0/"
if [ ! $? == 0 ]; then
	echo Packing using rar failed.
	exit 1
fi

# Inform success
mv ~/tmp/$version-bin.rar .
mv ~/tmp/$version-demo.rar .
echo Build and pack successful.
exit 0
