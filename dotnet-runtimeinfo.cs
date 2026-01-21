#!/usr/bin/env dotnet
#:property PackAsTool=true
#:property EnableSourceLink=true
#:package Microsoft.SourceLink.GitHub@*
#:package Spectre.Console@*

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using Spectre.Console;

const double Mebi = 1024 * 1024;
const double Gibi = Mebi * 1024;

var gcInfo = GC.GetGCMemoryInfo();
var totalMemoryBytes = gcInfo.TotalAvailableMemoryBytes;

// Create ASCII art
var logo = """
       [purple]▄▄[/]
      [purple]▀▀▀▀[/]
     [purple]██████[/]
    [purple]████████[/]
   [purple]██████████[/]
  [purple]██[/][white]████████[/][purple]██[/]
 [purple]███[/][white]██[/]  [purple]██[/]  [white]██[/][purple]███[/]
  [purple]█[/][white]██[/]   [purple]██[/]   [white]██[/][purple]█[/]
   [white]██[/]   [purple]██[/]   [white]██[/]
    [white]██[/]  [purple]██[/]  [white]██[/]
     [white]███████[/]
      [white]█████[/]
       [white]███[/]
       [white]███[/]
""";



// Create info panel
var infoLines = new List<string>
{
    $"[bold purple]{Environment.UserName}[/]@[bold purple]{Dns.GetHostName()}[/]",
    new string('─', 40),
    $"[bold]OS[/]: {RuntimeInformation.OSDescription}",
    $"[bold]Arch[/]: {RuntimeInformation.OSArchitecture}",
    $"[bold].NET[/]: {RuntimeInformation.FrameworkDescription}",
    $"[bold]CPU[/]: {Environment.ProcessorCount} cores",
    $"[bold]Memory[/]: {GetInBestUnit(totalMemoryBytes)}",
};

// Add cgroup info if available
if (OperatingSystem.IsLinux() && TryGetMemoryLimits(out long memoryLimit, out long currentMemory, out string? limitPath))
{
    infoLines.Add($"[bold]Container[/]: Yes");
    infoLines.Add($"[bold]Memory Limit[/]: {GetInBestUnit(memoryLimit)}");
    infoLines.Add($"[bold]Memory Used[/]: {GetInBestUnit(currentMemory)}");
    
    // Memory bar
    var percentage = (double)currentMemory / memoryLimit * 100;
    var barLength = 20;
    var filled = (int)(percentage / 100 * barLength);
    var bar = new string('█', filled) + new string('░', barLength - filled);
    var color = percentage > 80 ? "red" : percentage > 60 ? "yellow" : "green";
    infoLines.Add($"[bold]Usage[/]: [{color}]{bar}[/] {percentage:F1}%");
}

// Create layout
var table = new Table()
    .Border(TableBorder.None)
    .HideHeaders()
    .AddColumn(new TableColumn("").PadRight(2))
    .AddColumn(new TableColumn(""));

table.AddRow(logo, string.Join("\n", infoLines));

AnsiConsole.WriteLine();
AnsiConsole.Write(table);
AnsiConsole.WriteLine();

return 0;

static string GetInBestUnit(long size) => size switch
{
    < (long)Mebi => $"{size} bytes",
    < (long)Gibi => $"{size / Mebi:F1} MiB",
    _ => $"{size / Gibi:F1} GiB"
};

static bool TryGetMemoryLimits(out long limit, out long current, [NotNullWhen(true)] out string? limitPath)
{
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

    if (TryReadFirstLongFromPaths(memoryLimitPaths, out limit, out limitPath) && limit > 0)
    {
        TryReadFirstLongFromPaths(currentMemoryPaths, out current, out _);
        return true;
    }

    current = 0;
    return false;
}

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
