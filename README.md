# dotnet-runtimeinfo

A neofetch-style .NET tool that displays runtime, OS, and hardware environment information with ASCII art.

## Overview

`dotnet-runtimeinfo` prints detailed information about your .NET runtime, operating system, and hardware environment in a visually appealing neofetch-style format with dotnetbot ASCII art. It's useful for logging, diagnostics, and showing off your .NET setup.

## Installation

Install from NuGet:

```bash
dotnet tool install -g dotnet-runtimeinfo
dotnet-runtimeinfo
```

> Note: You may need to open a new terminal window after first installation.

Uninstall:

```bash
dotnet tool uninstall -g dotnet-runtimeinfo
```

## Usage

Simply run the command to see your system information:

```console
$ dotnet-runtimeinfo

       ▄▄          rich@hostname
      ▀▀▀▀         ────────────────────────────────
     ██████        OS: macOS 26.2.0
    ████████       Arch: Arm64
   ██████████      .NET: .NET 10.0.2
  ████████████     CPU: 12 cores
 █████  ██  █████  Memory: 24.0 GiB
  ███   ██   ███
   ██   ██   ██
    ██  ██  ██
     ███████
      █████
       ███
       ███
```

In containerized environments, additional cgroup information is displayed including memory limits and usage bars.

## Implementation

This is a .NET 10 single-file app using:
- `Spectre.Console` for colored output and layout
- `System.Runtime.InteropServices.RuntimeInformation` for platform details
- `System.Environment` for system information
- CGroup detection (v1 & v2) for containerized environments
- Memory usage visualization with progress bars
