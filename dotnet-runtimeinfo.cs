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

// Parse arguments
var cmdArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();
var logoStyle = cmdArgs.Length > 0 ? cmdArgs[0] : "dotnet";

var gcInfo = GC.GetGCMemoryInfo();
var totalMemoryBytes = gcInfo.TotalAvailableMemoryBytes;

// Logo options
var logos = new Dictionary<string, string>
{
    ["dotnet"] = """
        [purple]     _   _ _____ _____[/]
        [purple] _  | \\ | | ____||_   _|[/]
        [purple](_) |  \\| |  _|    | |  [/]
        [purple] _  | |\\  | |___   | |  [/]
        [purple](_) |_| \\_||_____|  |_|  [/]
        """,
    
    ["simple"] = """
        [purple]  ▄▄▄  ██▄  █ ████ ███▄[/]
        [purple] █   █ █ ██ █ █      █  [/]
        [purple] █   █ █  ███ ███    █  [/]
        [purple] █▄▄▄█ █   ██ █      █  [/]
        [purple]  ███  █    █ ████   █  [/]
        """,
    
    ["block"] = """
        [purple] ██████   ███  ██ ██████ ████████[/]
        [purple]▐█    █▌ ████  ██ ██        ██   [/]
        [purple]▐█    █▌ ██ ██ ██ ██████    ██   [/]
        [purple]▐█    █▌ ██  ████ ██        ██   [/]
        [purple] ██████  ██   ███ ██████    ██   [/]
        """,
    
    ["text"] = """
        [purple] ·NET[/]
        """
};

// Select logo or show help
if (logoStyle == "help" || logoStyle == "--help" || logoStyle == "-h")
{
    AnsiConsole.MarkupLine("[bold]Usage:[/] dotnet-runtimeinfo [logo-style]");
    AnsiConsole.MarkupLine("");
    AnsiConsole.MarkupLine("[bold]Logo styles:[/]");
    AnsiConsole.MarkupLine("  dotnet  - .NET logo with ASCII art (default)");
    AnsiConsole.MarkupLine("  simple  - Simple block style");
    AnsiConsole.MarkupLine("  block   - Bold block style");
    AnsiConsole.MarkupLine("  text    - Minimal text only");
    return 0;
}

var logo = logos.ContainsKey(logoStyle) ? logos[logoStyle] : logos["dotnet"];



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
