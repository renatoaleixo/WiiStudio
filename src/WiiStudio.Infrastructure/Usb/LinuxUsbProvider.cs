using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using WiiStudio.Domain.Entities;
using WiiStudio.Domain.Enums;
using WiiStudio.Infrastructure.Processes;
using WiiStudio.Infrastructure.Wiimms;

namespace WiiStudio.Infrastructure.Usb;

public sealed class LinuxUsbProvider : IUsbProvider
{
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(5);

    private readonly IProcessRunner? processRunner;
    private readonly IWiimmsToolLocator? wiimmsToolLocator;

    public LinuxUsbProvider(IProcessRunner? processRunner = null, IWiimmsToolLocator? wiimmsToolLocator = null)
    {
        this.processRunner = processRunner;
        this.wiimmsToolLocator = wiimmsToolLocator;
    }

    public async Task<IReadOnlyList<UsbDrive>> DetectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var drives = DriveInfo.GetDrives()
            .Where(IsCandidateDrive)
            .Select(MapMountedDrive)
            .ToList();

        drives.AddRange(await DetectRawWbfsPartitionsAsync(drives, cancellationToken));

        return drives
            .GroupBy(drive => drive.DevicePath, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
    }

    private static bool IsCandidateDrive(DriveInfo drive)
    {
        if (!drive.IsReady || drive.DriveType is not (DriveType.Removable or DriveType.Fixed))
        {
            return false;
        }

        var fileSystem = MapFileSystem(drive.DriveFormat);
        return fileSystem == UsbDriveFileSystem.Wbfs || HasWbfsDirectory(drive);
    }

    private static UsbDrive MapMountedDrive(DriveInfo drive)
    {
        var mountPoint = drive.RootDirectory.FullName;
        var label = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? drive.Name : drive.VolumeLabel;
        var freeSpace = drive.AvailableFreeSpace;
        var usedSpace = drive.TotalSize - freeSpace;

        return new UsbDrive(
            CreateStableId(mountPoint),
            mountPoint,
            mountPoint,
            label,
            MapFileSystem(drive.DriveFormat),
            drive.TotalSize,
            usedSpace,
            freeSpace,
            isMounted: true,
            isWritable: true);
    }

    private static bool HasWbfsDirectory(DriveInfo drive)
    {
        return Directory.Exists(Path.Combine(drive.RootDirectory.FullName, "wbfs"));
    }

    private async Task<IReadOnlyList<UsbDrive>> DetectRawWbfsPartitionsAsync(
        IReadOnlyList<UsbDrive> mountedDrives,
        CancellationToken cancellationToken)
    {
        if (processRunner is null || wiimmsToolLocator is null)
        {
            return [];
        }

        var lsblkPath = FindExecutable("lsblk");
        if (lsblkPath is null)
        {
            return [];
        }

        var lsblk = await processRunner.RunAsync(
            new ProcessRequest(
                lsblkPath,
                ["-P", "-b", "-o", "PATH,TYPE,FSTYPE,LABEL,SIZE,FSAVAIL,MOUNTPOINT,RO,MODEL"],
                ProbeTimeout),
            cancellationToken);
        if (!lsblk.IsSuccess)
        {
            return [];
        }

        string wwtPath;
        try
        {
            wwtPath = await wiimmsToolLocator.FindWwtAsync(cancellationToken);
        }
        catch
        {
            return [];
        }

        var mountedPaths = mountedDrives
            .Select(drive => drive.DevicePath)
            .Concat(mountedDrives.Select(drive => drive.MountPoint))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var drives = new List<UsbDrive>();

        foreach (var entry in ParseLsblkEntries(lsblk.StandardOutput))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsRawPartitionCandidate(entry, mountedPaths))
            {
                continue;
            }

            var wwt = await processRunner.RunAsync(
                new ProcessRequest(wwtPath, ["find", "--long", entry.Path], ProbeTimeout),
                cancellationToken);
            var probeOutput = $"{wwt.StandardOutput}\n{wwt.StandardError}";
            if (!probeOutput.Contains("BLOCK WBFS", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(entry.FileSystem, "wbfs", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            drives.Add(MapRawWbfsPartition(entry, probeOutput));
        }

        return drives;
    }

    private static bool IsRawPartitionCandidate(LsblkEntry entry, IReadOnlySet<string> mountedPaths)
    {
        return string.Equals(entry.Type, "part", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(entry.Path)
            && !mountedPaths.Contains(entry.Path)
            && string.IsNullOrWhiteSpace(entry.MountPoint)
            && entry.ReadOnly != "1"
            && (string.IsNullOrWhiteSpace(entry.FileSystem)
                || string.Equals(entry.FileSystem, "wbfs", StringComparison.OrdinalIgnoreCase));
    }

    private static UsbDrive MapRawWbfsPartition(LsblkEntry entry, string wwtOutput)
    {
        var totalSize = entry.SizeBytes > 0 ? entry.SizeBytes : ParseWwtSizeBytes(wwtOutput);
        var freeSpace = ParseWwtFreeBytes(wwtOutput) ?? totalSize;
        var usedSpace = Math.Max(0, totalSize - freeSpace);
        var label = !string.IsNullOrWhiteSpace(entry.Label)
            ? entry.Label
            : !string.IsNullOrWhiteSpace(entry.Model)
                ? $"{entry.Model} ({Path.GetFileName(entry.Path)})"
                : $"WBFS {entry.Path}";

        return new UsbDrive(
            CreateStableId(entry.Path),
            entry.Path,
            entry.Path,
            label,
            UsbDriveFileSystem.Wbfs,
            totalSize,
            usedSpace,
            freeSpace,
            isMounted: true,
            isWritable: entry.ReadOnly != "1");
    }

    private static long ParseWwtSizeBytes(string output)
    {
        var blockLine = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(line => line.Contains("BLOCK", StringComparison.OrdinalIgnoreCase)
                && line.Contains("WBFS", StringComparison.OrdinalIgnoreCase));
        if (blockLine is null)
        {
            return 0;
        }

        var parts = blockLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 4 && long.TryParse(parts[^2], out var mib)
            ? mib * 1024L * 1024L
            : 0;
    }

    private static long? ParseWwtFreeBytes(string output)
    {
        var totalLine = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(line => line.StartsWith("Total:", StringComparison.OrdinalIgnoreCase));
        if (totalLine is null)
        {
            return null;
        }

        var match = Regex.Match(totalLine, @"used,\s*(?<free>\d+)\s+MiB\s+~\s+.+free", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success && long.TryParse(match.Groups["free"].Value, out var mib)
            ? mib * 1024L * 1024L
            : null;
    }

    private static IReadOnlyList<LsblkEntry> ParseLsblkEntries(string output)
    {
        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseLsblkEntry)
            .Where(entry => entry.Path.Length > 0)
            .ToArray();
    }

    private static LsblkEntry ParseLsblkEntry(string line)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in SplitKeyValueTokens(line))
        {
            var separatorIndex = token.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = token[..separatorIndex];
            var value = token[(separatorIndex + 1)..].Trim('"');
            fields[key] = value;
        }

        return new LsblkEntry(
            fields.GetValueOrDefault("PATH") ?? string.Empty,
            fields.GetValueOrDefault("TYPE") ?? string.Empty,
            fields.GetValueOrDefault("FSTYPE") ?? string.Empty,
            fields.GetValueOrDefault("LABEL") ?? string.Empty,
            long.TryParse(fields.GetValueOrDefault("SIZE"), out var size) ? size : 0,
            fields.GetValueOrDefault("MOUNTPOINT") ?? string.Empty,
            fields.GetValueOrDefault("RO") ?? string.Empty,
            fields.GetValueOrDefault("MODEL") ?? string.Empty);
    }

    private static IReadOnlyList<string> SplitKeyValueTokens(string line)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var character in line)
        {
            if (character == '"')
            {
                inQuotes = !inQuotes;
            }

            if (character == ' ' && !inQuotes)
            {
                AddToken();
                continue;
            }

            current.Append(character);
        }

        AddToken();
        return tokens;

        void AddToken()
        {
            if (current.Length == 0)
            {
                return;
            }

            tokens.Add(current.ToString());
            current.Clear();
        }
    }

    private static string? FindExecutable(string executableName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return path
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(directory => Path.Combine(directory, executableName))
            .FirstOrDefault(File.Exists);
    }

    private static UsbDriveFileSystem MapFileSystem(string driveFormat)
    {
        return driveFormat.ToUpperInvariant() switch
        {
            "WBFS" => UsbDriveFileSystem.Wbfs,
            "FAT32" => UsbDriveFileSystem.Fat32,
            "NTFS" => UsbDriveFileSystem.Ntfs,
            "EXFAT" => UsbDriveFileSystem.ExFat,
            _ => UsbDriveFileSystem.Unknown
        };
    }

    private static Guid CreateStableId(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value.Trim().ToUpperInvariant()));
        return new Guid(hash[..16]);
    }

    private sealed record LsblkEntry(
        string Path,
        string Type,
        string FileSystem,
        string Label,
        long SizeBytes,
        string MountPoint,
        string ReadOnly,
        string Model);
}
