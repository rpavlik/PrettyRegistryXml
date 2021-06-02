#!/usr/bin/env pwsh
# Copyright 2021 Collabora, Ltd
#
# SPDX-License-Identifier: MIT

# dotnet clean
dotnet build

$repoBase = "https://github.com/rpavlik/PrettyRegistryXml"
xmldocmd core/bin/Debug/net5.0/PrettyRegistryXml.Core.dll docs --source ../core --clean
xmldocmd openxr/bin/Debug/net5.0/PrettyRegistryXml.OpenXR.dll docs --source ../openxr --clean
