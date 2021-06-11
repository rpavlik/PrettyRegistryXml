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

This uses the [.NET 5.0 SDK][dotnet5] (formerly known as .NET Core - the
cross-platform open source one) SDK to build. That link has instructions to
download and install it.

If you're on Windows or Mac, Visual Studio can handle this presumably, but I
wrote it on Linux using the CLI, so that's the instructions I'll provide.

In a terminal in the root directory of this repo:

```sh
dotnet build
```

will compile it. To run from the build tree, you need to specify the project you
want to execute (the solution file in this root doesn't know which one can be
executed):

```sh
# Add any arguments after the csproj
dotnet run --project src/PrettyRegistryXml.OpenXR/PrettyRegistryXml.OpenXR.csproj
```

By default, with no arguments you'll see help/usage.

**NOTE**: You do not want to just directly commit the modifications made by the
tool: please be selective in what you stage vs revert, using something like
`git gui`, `git add -p`, or other interactive/graphical/patch-based interface.
Some of the hand-formatted XML is better than the automated stuff.

[dotnet5]: https://dotnet.microsoft.com/download/dotnet/5.0

## API Docs

The XML documentation comments in the source can be used by your editor, as well
as turned into markdown documents:

All assemblies and docs are within `PrettyRegistryXml`:

- [`PrettyRegistryXml.Core`](docs/PrettyRegistryXml.Core.md)
  - Base functionality
- [`PrettyRegistryXml.GroupedAlignment`](docs/PrettyRegistryXml.GroupedAlignment.md)
  - Some more fancy alignment
- [`PrettyRegistryXml.OpenXR`](docs/PrettyRegistryXml.OpenXR.md)
  - OpenXR frontend - less complete docs but perhaps useful as an example.

When changes are merged into the main branch, these docs are automatically
updated and committed, if applicable, so no need to update them yourself.

You can re-generate these locally for you own reference, however, using
[XmlDocMarkdown](https://ejball.com/XmlDocMarkdown/) by doing the following:

```sh
# This script runs the tool through the local manifest.
./Build-Docs.ps1
```

## License

[REUSE](https://reuse.software) 3.0 compliant - use the `reuse` tool to get a
software BOM.

The code in this repo is MIT. This file is CC-BY-4.0. A few random data-ish
files are CC0-1.0.

Dependencies downloaded from NuGet include:

- [MoreLINQ][]: Apache-2.0
- [CommandLineParser][]: MIT (used only in the OpenXR entry point)

[MoreLINQ]: https://www.nuget.org/packages/morelinq/3.3.2
[CommandLineParser]: https://www.nuget.org/packages/CommandLineParser/2.9.0-preview1
