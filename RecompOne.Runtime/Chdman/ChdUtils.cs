
using System.Diagnostics;

namespace RecompOne.Runtime.Chdman;


public static class ChdUtils
{

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

            var chdmanPath = Path.Combine(
                AppContext.BaseDirectory,
                "chdman",
                "chdman.exe");

            var processInfo = new ProcessStartInfo
            {
                FileName = chdmanPath,
                Arguments = $"extractcd -i \"{path}\" -o \"{cuePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CHD conversion failed: {ex}");
            return false;
        }

        return true;
    }

    public static string GetCuePath(string chdPath)
    {
        return Path.ChangeExtension(chdPath, ".cue"); 
    }

}
