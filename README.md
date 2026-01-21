# dotnet-runtimeinfo

A neofetch-style .NET tool that displays runtime, OS, and hardware environment information with customizable ASCII art logos.

## Overview

`dotnet-runtimeinfo` prints detailed information about your .NET runtime, operating system, and hardware environment in a visually appealing neofetch-style format. It's useful for logging, diagnostics, and showing off your .NET setup.

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

Run with default logo:

```console
$ dotnet-runtimeinfo

     _   _ _____ _____      rich@hostname
 _  | \ | | ____||_   _|   ────────────────────────────────
(_) |  \| |  _|    | |     OS: macOS 26.2.0
 _  | |\ | | |___   | |     Arch: Arm64
(_) |_| \_||_____|  |_|    .NET: .NET 10.0.2
                            CPU: 12 cores
                            Memory: 24.0 GiB
```

### Logo Styles

Choose from multiple logo styles:

```bash
dotnet-runtimeinfo          # Default .NET ASCII logo
dotnet-runtimeinfo simple   # Simple block style
dotnet-runtimeinfo block    # Bold block style  
dotnet-runtimeinfo text     # Minimal text only
dotnet-runtimeinfo help     # Show all options
```

### Container Support

In containerized environments, additional cgroup information is displayed including memory limits and usage bars with color-coded visualization.

## Implementation

This is a .NET 10 single-file app using:
- `Spectre.Console` for colored output and layout
- `System.Runtime.InteropServices.RuntimeInformation` for platform details
- `System.Environment` for system information
- CGroup detection (v1 & v2) for containerized environments
- Memory usage visualization with progress bars
