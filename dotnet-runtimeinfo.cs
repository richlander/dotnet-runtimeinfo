#!/usr/bin/env dotnet
#:package Microsoft.SourceLink.GitHub@*
#:property Authors=Rich Lander
#:property Description=A neofetch-like tool for .NET metrics
#:property PackageId=dotnet-runtimeinfo
#:property PackageLicenseExpression=MIT
#:property PackageReadmeFile=README.md
#:property PublishRepositoryUrl=true
#:property PackAsTool=true
#:property RollForward=LatestMajor
#:property TargetFramework=net10.0
#:property ToolCommandName=dotnet-runtimeinfo
#:property VersionPrefix=2.0.0

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;

const double Mebi = 1024 * 1024;
const double Gibi = Mebi * 1024;

// ANSI color codes
const string Purple = "\e[35m";
const string White = "\e[37m";
const string Grey = "\e[90m";
const string Bold = "\e[1m";
const string BoldPurple = "\e[1;35m";
const string Reset = "\e[0m";
const string Red = "\e[31m";
const string Yellow = "\e[33m";
const string Green = "\e[32m";

// Parse arguments
var cmdArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();
var logoStyle = cmdArgs.Length > 0 ? cmdArgs[0] : "dotnetbot";

var gcInfo = GC.GetGCMemoryInfo();
var totalMemoryBytes = gcInfo.TotalAvailableMemoryBytes;

// Logo options (using ANSI codes)
var logos = new Dictionary<string, string[]>
{
    ["ascii"] = [
        $"{Purple}     _    _ _____  _____ {Reset}",
        $"{Purple}    | \\ | | ____||_   _|{Reset}",
        $"{Purple}    |  \\| |  _|    | |  {Reset}",
        $"{Purple}    | |\\  | |___   | |  {Reset}",
        $"{Purple}|\\| |_| \\_|_____|  |_|  {Reset}",
    ],

    ["blocktext"] = [
        $"{Purple}     ███  ██ ██████ ████████{Reset}",
        $"{Purple}     ████ ██ ██        ██   {Reset}",
        $"{Purple}     ██ ████ ██████    ██   {Reset}",
        $"{Purple} ███ ██  ███ ██        ██   {Reset}",
        $"{Purple} ███ ██   ██ ██████    ██   {Reset}",
    ],

    ["dotnetbot"] = [
        $"{Purple}             dNd{Reset}",
        $"{Purple}             dNd{Reset}",
        $"{Purple}         .dNNNNNNd.{Reset}",
        $"{Purple}       dNNNNNNNNNNNNd{Reset}",
        $"{Purple}      dNNNNNNNNNNNNNNNd{Reset}",
        $"{Purple}     dNNN{Reset}{White}.----------.{Reset}{Purple}NNNd{Reset}",
        $"{Purple}     dNNN{Reset}{White}|   ()   ()|{Reset}{Purple}NNNd{Reset}",
        $"{Purple}     dNNN{Reset}{White}'----------'{Reset}{Purple}NNNd{Reset}",
        $"{Purple}       dNNNNd    dNNNNd{Reset}",
        $"{Purple}        dNd {Reset}{Grey}|.NET|{Reset}{Purple} dNd{Reset}",
        $"{Purple}        dNd {Reset}{Grey}|    |{Reset}{Purple} dNd{Reset}",
        $"{Grey}            '----'{Reset}",
    ]
};

// Select logo or show help
if (logoStyle == "help" || logoStyle == "--help" || logoStyle == "-h")
{
    Console.WriteLine($"{Bold}Usage:{Reset} dotnet runtimeinfo [logo-style]");
    Console.WriteLine();
    Console.WriteLine($"{Bold}Logo styles:{Reset}");
    Console.WriteLine("  dotnetbot - .NET Bot mascot (default)");
    Console.WriteLine("  animated  - .NET Bot with eye animation");
    Console.WriteLine("  ascii     - .NET logo with ASCII art");
    Console.WriteLine("  blocktext - Block style .NET logo");
    return 0;
}

// Build info lines (shared between static and animated)
var infoLines = new List<string>
{
    $"{BoldPurple}{Environment.UserName}{Reset}@{BoldPurple}{Dns.GetHostName()}{Reset}",
    new string('─', 40),
    $"{Bold}.NET{Reset}: {GetDotnetVersion()}",
    $"{Bold}.NET SDK{Reset}: {GetDotnetSdkVersion()}",
    $"{Bold}Runtimes{Reset}: {CountDotnetRuntimes()}",
    $"{Bold}SDKs{Reset}: {CountDotnetSdks()}",
    "",
    $"{Bold}OS{Reset}: {RuntimeInformation.OSDescription}",
    $"{Bold}Arch{Reset}: {RuntimeInformation.OSArchitecture}",
    $"{Bold}CPU{Reset}: {Environment.ProcessorCount} cores",
    $"{Bold}Memory{Reset}: {GetInBestUnit(totalMemoryBytes)}",
};

// Add cgroup info if available
if (OperatingSystem.IsLinux() && TryGetMemoryLimits(out long memoryLimit, out long currentMemory, out string? limitPath))
{
    infoLines.Add($"{Bold}Container{Reset}: Yes");
    infoLines.Add($"{Bold}Memory Limit{Reset}: {GetInBestUnit(memoryLimit)}");
    infoLines.Add($"{Bold}Memory Used{Reset}: {GetInBestUnit(currentMemory)}");

    // Memory bar
    var percentage = (double)currentMemory / memoryLimit * 100;
    var barLength = 20;
    var filled = (int)(percentage / 100 * barLength);
    var bar = new string('█', filled) + new string('░', barLength - filled);
    var color = percentage > 80 ? Red : percentage > 60 ? Yellow : Green;
    infoLines.Add($"{Bold}Usage{Reset}: {color}{bar}{Reset} {percentage:F1}%");
}

// Handle animated dotnetbot
if (logoStyle == "animated")
{
    // Eyes: left, center, right - with shadow shifting based on "head turn"
    // Looking left: shadow on right side (light from left)
    string[] eyesLeft = [
        $"{Purple}              dNd{Reset}",
        $"{Purple}              dNd{Reset}",
        $"{Purple}          .dNNNNNN{Reset}{Grey}d.{Reset}",
        $"{Purple}        dNNNNNNNNNN{Reset}{Grey}NNd{Reset}",
        $"{Purple}       dNNNNNNNNNNNN{Reset}{Grey}NNNd{Reset}",
        $"{Purple}      dNNNN{Reset}{White}.----------.{Reset}{Grey}NNNd{Reset}",
        $"{Purple}      dNNNN{Reset}{White}|()   ()   |{Reset}{Grey}NNNd{Reset}",
        $"{Purple}      dNNNN{Reset}{White}'----------'{Reset}{Grey}NNNd{Reset}",
        $"{Purple}        dNNNNd   d{Reset}{Grey}NNNNd{Reset}",
    ];
    // Looking center: balanced lighting
    string[] eyesCenter = [
        $"{Purple}              dNd{Reset}",
        $"{Purple}              dNd{Reset}",
        $"{Purple}          .dNNNNNNd.{Reset}",
        $"{Purple}        dNNNNNNNNNNNNd{Reset}",
        $"{Purple}       dNNNNNNNNNNNNNNNd{Reset}",
        $"{Purple}      dNNN{Reset}{White}.-----------.{Reset}{Purple}NNNd{Reset}",
        $"{Purple}      dNNN{Reset}{White}|  ()   ()  |{Reset}{Purple}NNNd{Reset}",
        $"{Purple}      dNNN{Reset}{White}'-----------'{Reset}{Purple}NNNd{Reset}",
        $"{Purple}        dNNNNd    dNNNNd{Reset}",
    ];
    // Looking right: shadow on left side (light from right)
    string[] eyesRight = [
        $"{Purple}              dNd{Reset}",
        $"{Purple}              dNd{Reset}",
        $"{Grey}          .d{Reset}{Purple}NNNNNNd.{Reset}",
        $"{Grey}        dNN{Reset}{Purple}NNNNNNNNNNd{Reset}",
        $"{Grey}       dNNN{Reset}{Purple}NNNNNNNNNNNNd{Reset}",
        $"{Grey}      dNNN{Reset}{White}.----------.{Reset}{Purple}NNNNd{Reset}",
        $"{Grey}      dNNN{Reset}{White}|   ()   ()|{Reset}{Purple}NNNNd{Reset}",
        $"{Grey}      dNNN{Reset}{White}'----------'{Reset}{Purple}NNNNd{Reset}",
        $"{Grey}        dNNNN{Reset}{Purple}d   dNNNNd{Reset}",
    ];
    // Badge: up = |.NET| then |    |, down = |    | then |.NET|
    string[] badgeUp = [
        $"{Purple}         dNd {Reset}{Grey}|.NET|{Reset}{Purple} dNd{Reset}",
        $"{Purple}         dNd {Reset}{Grey}|    |{Reset}{Purple} dNd{Reset}",
        $"{Grey}             '----'{Reset}",
    ];
    string[] badgeDown = [
        $"{Purple}         dNd {Reset}{Grey}|    |{Reset}{Purple} dNd{Reset}",
        $"{Purple}         dNd {Reset}{Grey}|.NET|{Reset}{Purple} dNd{Reset}",
        $"{Grey}             '----'{Reset}",
    ];

    // Eye positions and badge states
    string[][] eyePositions = [eyesLeft, eyesCenter, eyesRight];

    // Animation settings
    int bounceCycles = 4;  // number of up/down cycles per eye position
    int bounceDelayMs = 150;  // delay between badge bounces
    int endBounceCycles = 2;  // extra bounces at the end

    Console.Write("\e[?25l"); // Hide cursor
    Console.WriteLine();

    bool firstFrame = true;
    int frameHeight = eyesLeft.Length + badgeUp.Length;

    try
    {
        // For each eye position (left, center, right)
        for (int eyePos = 0; eyePos < eyePositions.Length; eyePos++)
        {
            var eyes = eyePositions[eyePos];

            // Cycle badge up/down n times
            for (int cycle = 0; cycle < bounceCycles; cycle++)
            {
                // Up
                if (!firstFrame)
                {
                    Console.Write($"\e[{frameHeight}A");
                }
                firstFrame = false;

                var frameUp = eyes.Concat(badgeUp).ToArray();
                for (int i = 0; i < frameUp.Length; i++)
                {
                    string info = i < infoLines.Count ? "  " + infoLines[i] : "";
                    Console.WriteLine($"\e[2K{frameUp[i]}\e[32G{info}");
                }
                Thread.Sleep(bounceDelayMs);

                // Down
                Console.Write($"\e[{frameHeight}A");

                var frameDown = eyes.Concat(badgeDown).ToArray();
                for (int i = 0; i < frameDown.Length; i++)
                {
                    string info = i < infoLines.Count ? "  " + infoLines[i] : "";
                    Console.WriteLine($"\e[2K{frameDown[i]}\e[32G{info}");
                }

                if (cycle < bounceCycles - 1 || eyePos < eyePositions.Length - 1)
                {
                    Thread.Sleep(bounceDelayMs);
                }
            }
        }

        // Extra bounces at the end (eyes stay right)
        for (int cycle = 0; cycle < endBounceCycles; cycle++)
        {
            Thread.Sleep(bounceDelayMs);

            // Up
            Console.Write($"\e[{frameHeight}A");
            var frameUp = eyesRight.Concat(badgeUp).ToArray();
            for (int i = 0; i < frameUp.Length; i++)
            {
                string info = i < infoLines.Count ? "  " + infoLines[i] : "";
                Console.WriteLine($"\e[2K{frameUp[i]}\e[32G{info}");
            }
            Thread.Sleep(bounceDelayMs);

            // Down
            Console.Write($"\e[{frameHeight}A");
            var frameDown = eyesRight.Concat(badgeDown).ToArray();
            for (int i = 0; i < frameDown.Length; i++)
            {
                string info = i < infoLines.Count ? "  " + infoLines[i] : "";
                Console.WriteLine($"\e[2K{frameDown[i]}\e[32G{info}");
            }
        }

        Console.WriteLine();
    }
    finally
    {
        Console.Write("\e[?25h"); // Show cursor
    }
    return 0;
}

// Static logo output
var logoLines = logos.ContainsKey(logoStyle) ? logos[logoStyle] : logos["dotnetbot"];

Console.WriteLine();
int maxLines = Math.Max(logoLines.Length, infoLines.Count);
for (int i = 0; i < maxLines; i++)
{
    string logoLine = i < logoLines.Length ? logoLines[i] : "";
    string info = i < infoLines.Count ? "  " + infoLines[i] : "";
    Console.WriteLine($"{logoLine}\e[32G{info}");
}
Console.WriteLine();

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

static string GetDotnetSdkVersion()
{
    try
    {
        var psi = new ProcessStartInfo("dotnet", "--version")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        using var process = Process.Start(psi);
        return process?.StandardOutput.ReadToEnd().Trim() ?? "N/A";
    }
    catch
    {
        return "N/A";
    }
}

static int CountDotnetRuntimes()
{
    try
    {
        var psi = new ProcessStartInfo("dotnet", "--list-runtimes")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        using var process = Process.Start(psi);
        var output = process?.StandardOutput.ReadToEnd() ?? "";
        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
    }
    catch
    {
        return 0;
    }
}

static int CountDotnetSdks()
{
    try
    {
        var psi = new ProcessStartInfo("dotnet", "--list-sdks")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        using var process = Process.Start(psi);
        var output = process?.StandardOutput.ReadToEnd() ?? "";
        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
    }
    catch
    {
        return 0;
    }
}

static string GetDotnetVersion()
{
    // Extract just the version number from FrameworkDescription (e.g., ".NET 10.0.2" -> "10.0.2")
    var desc = RuntimeInformation.FrameworkDescription;
    var parts = desc.Split(' ');
    return parts.Length > 1 ? parts[^1] : desc;
}
