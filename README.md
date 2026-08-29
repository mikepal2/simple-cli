# SnapCLI
[![Build Status][ci-badge]][ci] [![NuGet][nuget-badge]![NuGet Downloads][nuget-download-badge]][nuget]

[ci]: https://github.com/mikepal2/snap-cli/actions?query=workflow%3ACI+branch%3Amain
[ci-badge]: https://github.com/mikepal2/snap-cli/workflows/CI/badge.svg
[nuget]: https://www.nuget.org/packages/SnapCLI/
[nuget-badge]: https://img.shields.io/nuget/v/SnapCLI.svg?style=flat-square
[nuget-download-badge]: https://img.shields.io/nuget/dt/SnapCLI?style=flat-square


Quickly create POSIX-like Command Line Interface (CLI) applications with a simple metadata API, built on top of the [System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/) library.

## NuGet Package

The library is available as a NuGet package:

- [SnapCLI](https://www.nuget.org/packages/SnapCLI/)

## Project Goal

While Microsoft's `System.CommandLine` library provides all the necessary APIs to parse command-line arguments, it requires significant effort to set up the code responsible for command-line handling before the program is ready to run. Additionally, this code can be difficult to maintain. For more context, see the [Motivation](https://github.com/mikepal2/snap-cli/blob/main/docs/Motivation.md) page.

The goal of this project is to address these issues by providing developers with easy-to-use mechanisms, while retaining the core functionality and features of `System.CommandLine`.

This library enables developers to quickly create POSIX-like CLI applications by automatically managing command-line commands and parameters using the provided metadata. This simplifies the development process and allows developers to focus on their application logic.

Additionally, it streamlines the creation of the application's help system, ensuring that all necessary information is easily accessible to end users.

The inspiration for this project came from the [DragonFruit](https://github.com/dotnet/command-line-api/blob/38414ef9c2aaa2f0c74d4170e0c0b13f4f60ad2b/docs/DragonFruit-overview.md) project, which was a step in the right direction to simplify the usage of `System.CommandLine` but has significant limitations.

## Documentation

Visit the [Documentation](https://github.com/mikepal2/snap-cli/blob/main/docs/Documentation.md) page to get started with SnapCLI’s APIs.

Step-by-step tutorials:

- [Building your first app with SnapCLI](https://github.com/mikepal2/snap-cli/blob/main/docs/Your-First-SnapCLI-App.md) — a single-task CLI driven by a parameterized `Main()`.
- [Building a multi-command CLI](https://github.com/mikepal2/snap-cli/blob/main/docs/Multi-Command-SnapCLI-App.md) — several commands, options and arguments in one app.

## Examples

There are several [samples](https://github.com/mikepal2/snap-cli/blob/main/samples/readme.md) provided to demonstrate various ways to use the library.

## .NET Framework Support

SnapCLI targets `netstandard2.0` and `net8.0`, matching the frameworks supported by `System.CommandLine`. The goal is to maintain the same level of support as that library. The current list is also shown on the [SnapCLI NuGet page](https://www.nuget.org/packages/SnapCLI#supportedframeworks-body-tab).

## License

This project is licensed under the [MIT License](https://github.com/mikepal2/snap-cli/blob/main/LICENSE.md). Some parts of this project are borrowed with modifications from [DragonFruit](https://github.com/dotnet/command-line-api/tree/38414ef9c2aaa2f0c74d4170e0c0b13f4f60ad2b/src/System.CommandLine.DragonFruit/targets) under the [MIT License](https://github.com/mikepal2/snap-cli/blob/main/LICENSE-command-line-api.md).
