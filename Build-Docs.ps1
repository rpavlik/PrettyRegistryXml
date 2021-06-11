#!/usr/bin/env pwsh
# Copyright 2021 Collabora, Ltd
#
# SPDX-License-Identifier: MIT

# dotnet clean
dotnet restore
dotnet tool restore
dotnet build

$dirs = Get-ChildItem src -Exclude *.Tests
foreach ($srcdir in $dirs) {
    # Write-Output $srcdir
    $filename = $srcdir.BaseName + ".dll"
    $pathToSources = "../src/" + $srcdir.BaseName
    $assemblypath = Join-Path $srcdir "bin/Debug/net5.0/$filename"
    Write-Output $assemblypath
    dotnet tool run xmldocmd $assemblypath docs --source $pathToSources --clean
}
