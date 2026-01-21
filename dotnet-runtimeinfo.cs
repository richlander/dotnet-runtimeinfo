#!/usr/bin/env dotnet
#:property PackAsTool=true
#:property EnableSourceLink=true
#:package Microsoft.SourceLink.GitHub@*
#:package Spectre.Console@*

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using Spectre.Console;

const double Mebi = 1024 * 1024;
const double Gibi = Mebi * 1024;

// Parse arguments
var cmdArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();
var logoStyle = cmdArgs.Length > 0 ? cmdArgs[0] : "dotnetbot";

var gcInfo = GC.GetGCMemoryInfo();
var totalMemoryBytes = gcInfo.TotalAvailableMemoryBytes;

// Logo options
var logos = new Dictionary<string, string>
{
    ["ascii"] = """
        [purple]     _    _ _____  _____[/]
        [purple]    | \\ | | ____||_   _|[/]
        [purple]    |  \\| |  _|    | |[/]
        [purple]    | |\\  | |___   | |[/]
        [purple]|\| |_| \\_|_____|  |_|[/]
        """,

    ["blocktext"] = """
        [purple]     ███  ██ ██████ ████████[/]
        [purple]     ████ ██ ██        ██[/]
        [purple]     ██ ████ ██████    ██[/]
        [purple]████ ██  ███ ██        ██[/]
        [purple]████ ██   ██ ██████    ██[/]
        """,

    ["dotnetbot"] = """
        [purple]             dNd[/]
        [purple]             dNd[/]
        [purple]         .dNNNNNNd.[/]
        [purple]       dNNNNNNNNNNNNd[/]
        [purple]      dNNNNNNNNNNNNNNNd[/]
        [purple]     dNNN[/][white].----------.[/][purple]NNNd[/]
        [purple]     dNNN[/][white]|   ()   ()|[/][purple]NNNd[/]
        [purple]     dNNN[/][white]'----------'[/][purple]NNNd[/]
        [purple]       dNNNNd    dNNNNd[/]
        [purple]        dNd [/][grey]|.NET|[/][purple] dNd[/]
        [purple]        dNd [/][grey]|    |[/][purple] dNd[/]
        [grey]            '----'[/]
        """
};

// Animated dotnetbot frames (eyes: left, center, right)
string[] dotnetbotFrames = [
    // Looking left
    """
    [purple]              dNd[/]
    [purple]              dNd[/]
    [purple]          .dNNNNNNd.[/]
    [purple]        dNNNNNNNNNNNNd[/]
    [purple]       dNNNNNNNNNNNNNNNd[/]
    [purple]      dNNN[/][grey].-----------.NNNd[/]
    [purple]      dNNN[/][grey]|()     ()  |NNNd[/]
    [purple]      dNNN[/][grey]'-----------'NNNd[/]
    [purple]        dNNNNd    dNNNNd[/]
    [purple]         dNd [/][grey]|.NET|[/][purple] dNd[/]
    [purple]         dNd [/][grey]|    |[/][purple] dNd[/]
    [grey]             '----'[/]
    """,
    // Looking center
    """
    [purple]              dNd[/]
    [purple]              dNd[/]
    [purple]          .dNNNNNNd.[/]
    [purple]        dNNNNNNNNNNNNd[/]
    [purple]       dNNNNNNNNNNNNNNNd[/]
    [purple]      dNNN[/][grey].-----------.NNNd[/]
    [purple]      dNNN[/][grey]|  ()   ()  |NNNd[/]
    [purple]      dNNN[/][grey]'-----------'NNNd[/]
    [purple]        dNNNNd    dNNNNd[/]
    [purple]         dNd [/][grey]|.NET|[/][purple] dNd[/]
    [purple]         dNd [/][grey]|    |[/][purple] dNd[/]
    [grey]             '----'[/]
    """,
    // Looking right
    """
    [purple]              dNd[/]
    [purple]              dNd[/]
    [purple]          .dNNNNNNd.[/]
    [purple]        dNNNNNNNNNNNNd[/]
    [purple]       dNNNNNNNNNNNNNNNd[/]
    [purple]      dNNN[/][grey].-----------.NNNd[/]
    [purple]      dNNN[/][grey]|   ()     ()|NNNd[/]
    [purple]      dNNN[/][grey]'-----------'NNNd[/]
    [purple]        dNNNNd    dNNNNd[/]
    [purple]         dNd [/][grey]|.NET|[/][purple] dNd[/]
    [purple]         dNd [/][grey]|    |[/][purple] dNd[/]
    [grey]             '----'[/]
    """
];

// Select logo or show help
if (logoStyle == "help" || logoStyle == "--help" || logoStyle == "-h")
{
    AnsiConsole.MarkupLine("[bold]Usage:[/] dotnet-runtimeinfo [[logo-style]]");
    AnsiConsole.MarkupLine("");
    AnsiConsole.MarkupLine("[bold]Logo styles:[/]");
    AnsiConsole.MarkupLine("  dotnetbot - .NET Bot mascot (default)");
    AnsiConsole.MarkupLine("  animated  - .NET Bot with eye animation");
    AnsiConsole.MarkupLine("  ascii     - .NET logo with ASCII art");
    AnsiConsole.MarkupLine("  blocktext - Block style .NET logo");
    return 0;
}

// Handle animated dotnetbot
if (logoStyle == "animated")
{
    var animInfoLines = new[]
    {
        $"\e[1;35m{Environment.UserName}\e[0m@\e[1;35m{Dns.GetHostName()}\e[0m",
        new string('─', 40),
        $"\e[1m.NET\e[0m: {GetDotnetVersion()}",
        $"\e[1m.NET SDK\e[0m: {GetDotnetSdkVersion()}",
        $"\e[1mRuntimes\e[0m: {CountDotnetRuntimes()}",
        $"\e[1mSDKs\e[0m: {CountDotnetSdks()}",
        "",
        $"\e[1mOS\e[0m: {RuntimeInformation.OSDescription}",
        $"\e[1mArch\e[0m: {RuntimeInformation.OSArchitecture}",
        $"\e[1mCPU\e[0m: {Environment.ProcessorCount} cores",
        $"\e[1mMemory\e[0m: {GetInBestUnit(totalMemoryBytes)}",
    };

    // Raw frames without Spectre markup (using ANSI codes directly)
    // Eyes: left, center, right - with shadow shifting based on "head turn"
    // Looking left: shadow on right side (light from left)
    string[] eyesLeft = [
        "\e[35m              dNd\e[0m",
        "\e[35m              dNd\e[0m",
        "\e[35m          .dNNNNNN\e[0m\e[90md.\e[0m",
        "\e[35m        dNNNNNNNNNN\e[0m\e[90mNNd\e[0m",
        "\e[35m       dNNNNNNNNNNNN\e[0m\e[90mNNNd\e[0m",
        "\e[35m      dNNNN\e[0m\e[37m.----------.\e[0m\e[90mNNNd\e[0m",
        "\e[35m      dNNNN\e[0m\e[37m|()   ()   |\e[0m\e[90mNNNd\e[0m",
        "\e[35m      dNNNN\e[0m\e[37m'----------'\e[0m\e[90mNNNd\e[0m",
        "\e[35m        dNNNNd   d\e[0m\e[90mNNNNd\e[0m",
    ];
    // Looking center: balanced lighting
    string[] eyesCenter = [
        "\e[35m              dNd\e[0m",
        "\e[35m              dNd\e[0m",
        "\e[35m          .dNNNNNNd.\e[0m",
        "\e[35m        dNNNNNNNNNNNNd\e[0m",
        "\e[35m       dNNNNNNNNNNNNNNNd\e[0m",
        "\e[35m      dNNN\e[0m\e[37m.-----------.\e[0m\e[35mNNNd\e[0m",
        "\e[35m      dNNN\e[0m\e[37m|  ()   ()  |\e[0m\e[35mNNNd\e[0m",
        "\e[35m      dNNN\e[0m\e[37m'-----------'\e[0m\e[35mNNNd\e[0m",
        "\e[35m        dNNNNd    dNNNNd\e[0m",
    ];
    // Looking right: shadow on left side (light from right)
    string[] eyesRight = [
        "\e[35m              dNd\e[0m",
        "\e[35m              dNd\e[0m",
        "\e[90m          .d\e[0m\e[35mNNNNNNd.\e[0m",
        "\e[90m        dNN\e[0m\e[35mNNNNNNNNNNd\e[0m",
        "\e[90m       dNNN\e[0m\e[35mNNNNNNNNNNNNd\e[0m",
        "\e[90m      dNNN\e[0m\e[37m.----------.\e[0m\e[35mNNNNd\e[0m",
        "\e[90m      dNNN\e[0m\e[37m|   ()   ()|\e[0m\e[35mNNNNd\e[0m",
        "\e[90m      dNNN\e[0m\e[37m'----------'\e[0m\e[35mNNNNd\e[0m",
        "\e[90m        dNNNN\e[0m\e[35md   dNNNNd\e[0m",
    ];
    // Badge: up = |.NET| then |    |, down = |    | then |.NET|
    string[] badgeUp = [
        "\e[35m         dNd \e[0m\e[90m|.NET|\e[0m\e[35m dNd\e[0m",
        "\e[35m         dNd \e[0m\e[90m|    |\e[0m\e[35m dNd\e[0m",
        "\e[90m             '----'\e[0m",
    ];
    string[] badgeDown = [
        "\e[35m         dNd \e[0m\e[90m|    |\e[0m\e[35m dNd\e[0m",
        "\e[35m         dNd \e[0m\e[90m|.NET|\e[0m\e[35m dNd\e[0m",
        "\e[90m             '----'\e[0m",
    ];

    // Eye positions and badge states
    string[][] eyePositions = [eyesLeft, eyesCenter, eyesRight];
    string[][] badgeStates = [badgeUp, badgeDown];

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
                    string info = i < animInfoLines.Length ? "  " + animInfoLines[i] : "";
                    Console.WriteLine($"\e[2K{frameUp[i]}\e[32G{info}");
                }
                Thread.Sleep(bounceDelayMs);

                // Down
                Console.Write($"\e[{frameHeight}A");

                var frameDown = eyes.Concat(badgeDown).ToArray();
                for (int i = 0; i < frameDown.Length; i++)
                {
                    string info = i < animInfoLines.Length ? "  " + animInfoLines[i] : "";
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
                string info = i < animInfoLines.Length ? "  " + animInfoLines[i] : "";
                Console.WriteLine($"\e[2K{frameUp[i]}\e[32G{info}");
            }
            Thread.Sleep(bounceDelayMs);

            // Down
            Console.Write($"\e[{frameHeight}A");
            var frameDown = eyesRight.Concat(badgeDown).ToArray();
            for (int i = 0; i < frameDown.Length; i++)
            {
                string info = i < animInfoLines.Length ? "  " + animInfoLines[i] : "";
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

var logo = logos.ContainsKey(logoStyle) ? logos[logoStyle] : logos["ascii"];



// Create info panel
var infoLines = new List<string>
{
    $"[bold purple]{Environment.UserName}[/]@[bold purple]{Dns.GetHostName()}[/]",
    new string('─', 40),
    $"[bold].NET[/]: {GetDotnetVersion()}",
    $"[bold].NET SDK[/]: {GetDotnetSdkVersion()}",
    $"[bold]Runtimes[/]: {CountDotnetRuntimes()}",
    $"[bold]SDKs[/]: {CountDotnetSdks()}",
    "",
    $"[bold]OS[/]: {RuntimeInformation.OSDescription}",
    $"[bold]Arch[/]: {RuntimeInformation.OSArchitecture}",
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
