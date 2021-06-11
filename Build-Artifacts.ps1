#!/usr/bin/env pwsh
# Copyright 2021 Collabora, Ltd
#
# SPDX-License-Identifier: MIT

if (-not(Test-Path Env:BUILD_OS)) {
    $Env:BUILD_OS = "ubuntu-latest"
}
$Env:build_version = (git describe).Trim("v")
if (Test-Path Env:BUILD_REF) {
    if ("$Env:BUILD_REF".StartsWith("refs/tags/v")) {
        $Env:build_version = "$Env:BUILD_REF".Trim("refs/tags/v")
    }
}

$Version = $Env:build_version
$ArtifactName = "PrettyRegistryXml-${Env:BUILD_OS}-$Version"

if (Test-Path Env:GITHUB_ENV) {
    # Provide these variables to the following actions as well
    Write-Output "build_version=$Env:build_version" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
    Write-Output "ARTIFACT_NAME=$ArtifactName" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
}

dotnet build -c Release "-property:Version=$Version" "-property:AssemblyFileVersion=$Version" "-property:AssemblyInformationalVersionVersion=$Version"
dotnet publish -c Release -o $ArtifactName/ "-property:Version=$Version"  "-property:AssemblyFileVersion=$Version" "-property:AssemblyInformationalVersionVersion=$Version"

# Put the licenses in there too.
Copy-Item -Recurse LICENSES $ArtifactName/
Copy-Item LICENSE.md $ArtifactName/LICENSE.txt

# Make sure directory exists
if (-not(Test-Path -PathType Container -Path out)) {
    New-Item -ItemType Directory -Path out
}

7z a -bd -r out/$ArtifactName.7z $ArtifactName/
