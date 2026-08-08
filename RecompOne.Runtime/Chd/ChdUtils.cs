using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RecompOne.Runtime.Chd;

public static class ChdUtils
{
    private static readonly string[] _loadingFrames = { "|", "/", "-", "\\" };
    private static int _loadingFrame;
    private static DateTime _lastLoadingUpdate = DateTime.UtcNow;

    public static bool UnpackChd(string path)
    {
        try
        {
            var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(path))!;
            var fileName = Path.GetFileNameWithoutExtension(path);

            var cuePath = Path.Combine(outputDirectory, fileName + ".cue");

            // Already unpacked, nothing to do.
            if (File.Exists(cuePath))
                return true;

            var chdmanPath = GetChdmanPath();

            Console.WriteLine($"Using chdman: {chdmanPath}");

            if (!File.Exists(chdmanPath))
            {
                Console.WriteLine($"chdman executable not found: {chdmanPath}");
                return false;
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = chdmanPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            processInfo.ArgumentList.Add("extractcd");
            processInfo.ArgumentList.Add("-i");
            processInfo.ArgumentList.Add(path);
            processInfo.ArgumentList.Add("-o");
            processInfo.ArgumentList.Add(cuePath);

            using var process = Process.Start(processInfo);

            if (process == null)
                return false;

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            Console.WriteLine(output);

            if (process.ExitCode != 0)
            {
                Console.WriteLine(error);
                return false;
            }

            if (!File.Exists(cuePath))
                return false;

            Console.WriteLine($"CHD converted to: {cuePath}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CHD conversion failed: {ex}");
            return false;
        }
    }

    private static string GetChdmanPath()
    {
        var os = GetOperatingSystem();
        var architecture = GetArchitecture();

        var executable = OperatingSystem.IsWindows()
            ? "chdman.exe"
            : "chdman";

        return Path.Combine(
            AppContext.BaseDirectory,
            "chd",
            "chdman",
            os,
            architecture,
            executable); 
    } 

    private static string GetOperatingSystem()
    {
        if (OperatingSystem.IsWindows())
            return "Windows";

        if (OperatingSystem.IsLinux())
            return "Linux";

        if (OperatingSystem.IsMacOS())
            return "MacOS";

        throw new PlatformNotSupportedException(
            $"CHD unpacking is not supported on {RuntimeInformation.OSDescription}.");
    }

    private static string GetArchitecture()
    {
        return RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",

            _ => throw new PlatformNotSupportedException(
                $"CHD unpacking is not supported on architecture " +
                $"{RuntimeInformation.OSArchitecture}.")
        };
    }

    public static string GetCuePath(string chdPath)
    {
        return Path.ChangeExtension(chdPath, ".cue");
    }

    public static string UpdateLoadingAnimation()
    {
        var now = DateTime.UtcNow;

        if ((now - _lastLoadingUpdate).TotalMilliseconds >= 100)
        {
            _loadingFrame = (_loadingFrame + 1) % _loadingFrames.Length;
            _lastLoadingUpdate = now;
        }

        return $"Verifying and unpacking CHD {_loadingFrames[_loadingFrame]}";
    }

    public static void ResetLoadingAnimation()
    {
        _loadingFrame = 0;
        _lastLoadingUpdate = DateTime.UtcNow;
    }
}