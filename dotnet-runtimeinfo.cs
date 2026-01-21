#!/usr/bin/env dotnet
#:property PackAsTool=true
#:property EnableSourceLink=true
#:package Microsoft.SourceLink.GitHub@*

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;

const double Mebi = 1024 * 1024;
const double Gibi = Mebi * 1024;

var gcInfo = GC.GetGCMemoryInfo();
var totalMemoryBytes = gcInfo.TotalAvailableMemoryBytes;

// OS and .NET information
Console.WriteLine($"{nameof(RuntimeInformation.OSArchitecture)}: {RuntimeInformation.OSArchitecture}");
Console.WriteLine($"{nameof(RuntimeInformation.OSDescription)}: {RuntimeInformation.OSDescription}");
Console.WriteLine($"{nameof(RuntimeInformation.FrameworkDescription)}: {RuntimeInformation.FrameworkDescription}");
Console.WriteLine();

// Environment information
Console.WriteLine($"{nameof(Environment.UserName)}: {Environment.UserName}");
Console.WriteLine($"HostName : {Dns.GetHostName()}");
Console.WriteLine();

// Hardware information
Console.WriteLine($"{nameof(Environment.ProcessorCount)}: {Environment.ProcessorCount}");
Console.WriteLine($"{nameof(GCMemoryInfo.TotalAvailableMemoryBytes)}: {totalMemoryBytes} ({GetInBestUnit(totalMemoryBytes)})");

string[] memoryLimitPaths =
[
    "/sys/fs/cgroup/memory.max",
    "/sys/fs/cgroup/memory.high",
    "/sys/fs/cgroup/memory.low",
    "/sys/fs/cgroup/memory/memory.limit_in_bytes",
];

string[] currentMemoryPaths =
[
    "/sys/fs/cgroup/memory.current",
    "/sys/fs/cgroup/memory/memory.usage_in_bytes",
];

// cgroup information
if (OperatingSystem.IsLinux()
    && TryReadFirstLongFromPaths(memoryLimitPaths, out long memoryLimit, out string? bestMemoryLimitPath)
    && memoryLimit > 0)
{
    TryReadFirstLongFromPaths(currentMemoryPaths, out long currentMemory, out _);

    Console.WriteLine($"cgroup memory constraint: {bestMemoryLimitPath}");
    Console.WriteLine($"cgroup memory limit: {memoryLimit} ({GetInBestUnit(memoryLimit)})");
    Console.WriteLine($"cgroup memory usage: {currentMemory} ({GetInBestUnit(currentMemory)})");
    Console.WriteLine($"GC Hard limit %: {(double)totalMemoryBytes / memoryLimit * 100:N0}");
}

return 0;

static string GetInBestUnit(long size) => size switch
{
    < (long)Mebi => $"{size} bytes",
    < (long)Gibi => $"{size / Mebi:F} MiB",
    _ => $"{size / Gibi:F} GiB"
};

static bool TryReadFirstLongFromPaths(string[] paths, out long limit, [NotNullWhen(true)] out string? bestPath)
{
    foreach (string path in paths)
    {
        if (File.Exists(path) && long.TryParse(File.ReadAllText(path), out limit))
        {
            bestPath = path;
            return true;
        }
    }

    bestPath = null;
    limit = 0;
    return false;
}
