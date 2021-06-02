# Pretty Registry (for OpenXR)

<!--
Copyright 2021 Collabora, Ltd

SPDX-License-Identifier: CC-BY-4.0
-->

A tool to perform (supervised!) formatting of the OpenXR registry XML, and
potentially other registry XML in the future.

It is split into `core`, which provides utilities and base classes, and `openxr`
which includes the "policy"-related code parts for OpenXR, as well as the entry
point.

## Building and Running

This uses the [.NET 5.0 SDK][dotnet5] (formerly known as .NET Core - the cross-platform open source one) SDK to build. That link has instructions to download and install it.

If you're on Windows or Mac, Visual Studio can handle this presumably, but I
wrote it on Linux using the CLI, so that's the instructions I'll provide.

In a terminal in the root directory of this repo:

```sh
dotnet build
```

will compile it. To run from the build tree, you need to specify the project you want to execute (the solution file in this root doesn't know which one can be executed):

```sh
# Add any arguments after the csproj
dotnet run --project openxr/pretty-registry.openxr.csproj
```

By default, with no arguments you'll see help/usage.

**NOTE**: You do not want to just directly commit the modifications made by the
tool: please be selective in what you stage vs revert, using something like
`git gui`, `git add -p`, or other interactive/graphical/patch-based interface.
Some of the hand-formatted XML is better than the automated stuff.

[dotnet5]: https://dotnet.microsoft.com/download/dotnet/5.0

## API Docs

The `PrettyRegistryXml.Core` assembly has fairly complete XML documentation
comments, that can be used by your editor, etc. A Markdown version of them is
[committed and included](docs/PrettyRegistryXml.Core.md), along with the
[docs for the OpenXR assembly](docs/PrettyRegistryXml.OpenXR.md) which
are less complete but perhaps useful as an example. You can re-generate these
locally using [XmlDocMarkdown](https://ejball.com/XmlDocMarkdown/):

```sh
# This script runs the tool through the local manifest.
./BuildDocs.ps1
```

## License

[REUSE](https://reuse.software) 3.0 compliant - use the `reuse` tool to get a
software BOM.

The code in this repo is MIT. This file is CC-BY-4.0. A few random data-ish
files are CC0-1.0.

Dependencies downloaded from NuGet include:

- [MoreLINQ][]: Apache-2.0
- [CommandLineParser][]: MIT (used only in the entry point, of course)

[MoreLINQ]: https://www.nuget.org/packages/morelinq/3.3.2
[CommandLineParser]: https://www.nuget.org/packages/CommandLineParser/2.9.0-preview1
