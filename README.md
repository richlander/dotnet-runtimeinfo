# dotnet-runtimeinfo

A .NET tool that displays runtime, OS, and hardware environment information.

## Overview

`dotnet-runtimeinfo` prints detailed information about your .NET runtime, operating system, and hardware environment. It demonstrates the APIs available for retrieving system information useful for logging and diagnostics.

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

```console
dotnet-runtimeinfo
**.NET information
Version: 10.0.0
FrameworkDescription: .NET 10.0.0
Libraries version: 10.0.0
Libraries hash: abc123def456

**Environment information
OSDescription: Darwin 23.0.0 Darwin Kernel Version 23.0.0
OSVersion: Unix 23.0.0.0
OSArchitecture: Arm64
ProcessorCount: 12
```

## Implementation

This is a .NET 10 single-file app using:
- `System.Runtime.InteropServices.RuntimeInformation` for platform details
- `System.Environment` for system information
- CGroup detection for containerized environments
