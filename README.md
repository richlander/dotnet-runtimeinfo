# dotnet-runtimeinfo

A [neofetch](https://github.com/dylanaraps/neofetch)-style .NET tool that displays runtime, OS, and hardware environment information with customizable ASCII art logos.

## Overview

`dotnet-runtimeinfo` prints detailed information about your .NET runtime, operating system, and hardware environment in a visually appealing neofetch-style format. It's useful for logging, diagnostics, and showing off your .NET setup.

## Installation

Install from NuGet:

```bash
dotnet tool install -g dotnet-runtimeinfo
dotnet runtimeinfo
```

> Note: You may need to open a new terminal window after first installation.

Uninstall:

```bash
dotnet tool uninstall -g dotnet-runtimeinfo
```

## Usage

Run with default logo:

```console
$ dotnet runtimeinfo

             dNd             rich@hostname
             dNd             ────────────────────────────────
         .dNNNNNNd.          .NET: 10.0.2
       dNNNNNNNNNNNNd        .NET SDK: 10.0.102
      dNNNNNNNNNNNNNNNd      Runtimes: 20
     dNNN.----------.NNNd    SDKs: 10
     dNNN|   ()   ()|NNNd
     dNNN'----------'NNNd    OS: macOS 26.2.0
       dNNNNd    dNNNNd      Arch: Arm64
        dNd |.NET| dNd       CPU: 12 cores
        dNd |    | dNd       Memory: 24.0 GiB
            '----'
```

### Logo Styles

Choose from multiple logo styles:

```bash
dotnet runtimeinfo           # .NET Bot mascot (default)
dotnet runtimeinfo animated  # .NET Bot with eye animation
dotnet runtimeinfo ascii     # .NET logo with ASCII art
dotnet runtimeinfo blocktext # Block style .NET logo
dotnet runtimeinfo help      # Show all options
```

### Container Support

In containerized environments, additional cgroup information is displayed including memory limits and usage bars with color-coded visualization.

## Implementation

This is a .NET 10 single-file app using:
- ANSI escape codes for colored output
- `System.Runtime.InteropServices.RuntimeInformation` for platform details
- `System.Environment` for system information
- CGroup detection (v1 & v2) for containerized environments
- Memory usage visualization with progress bars
