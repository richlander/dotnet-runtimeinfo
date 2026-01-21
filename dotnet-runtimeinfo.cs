#!/usr/bin/env dotnet
#:property PackAsTool=true
#:property EnableSourceLink=true
#:package Microsoft.SourceLink.GitHub@*

using System.Reflection;
using System.Runtime.InteropServices;

var assemblyInformation = ((AssemblyInformationalVersionAttribute[])typeof(object).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false))[0];
var informationalVersionSplit = assemblyInformation.InformationalVersion.Split('+');

Console.WriteLine("**.NET information");
Console.WriteLine($"{nameof(Environment.Version)}: {Environment.Version}");
Console.WriteLine($"{nameof(RuntimeInformation.FrameworkDescription)}: {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Libraries version: {informationalVersionSplit[0]}");
Console.WriteLine($"Libraries hash: {informationalVersionSplit[1]}");
Console.WriteLine();
Console.WriteLine("**Environment information");
Console.WriteLine($"{nameof(RuntimeInformation.OSDescription)}: {RuntimeInformation.OSDescription}");
Console.WriteLine($"{nameof(Environment.OSVersion)}: {Environment.OSVersion}");
Console.WriteLine($"{nameof(RuntimeInformation.OSArchitecture)}: {RuntimeInformation.OSArchitecture}");
Console.WriteLine($"{nameof(Environment.ProcessorCount)}: {Environment.ProcessorCount}");
Console.WriteLine();

if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && 
    Directory.Exists("/sys/fs/cgroup/cpu") &&
    Directory.Exists("/sys/fs/cgroup/memory"))
{
    Console.WriteLine("**CGroup info");
    Console.WriteLine($"cfs_quota_us: {File.ReadAllLines("/sys/fs/cgroup/cpu/cpu.cfs_quota_us")[0]}");
    Console.WriteLine($"memory.limit_in_bytes: {File.ReadAllLines("/sys/fs/cgroup/memory/memory.limit_in_bytes")[0]}");
    Console.WriteLine($"memory.usage_in_bytes: {File.ReadAllLines("/sys/fs/cgroup/memory/memory.usage_in_bytes")[0]}");
}

return 0;
