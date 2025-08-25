using Spectre.Console;

namespace ShipMvp.Cli.src.Services;

public sealed class Shell
{
    public async Task Run(string file, params string[] args)
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = file,
            Arguments = string.Join(" ", args.Select(Escape)),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var p = System.Diagnostics.Process.Start(psi)!;
        var stdout = await p.StandardOutput.ReadToEndAsync();
        var stderr = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        if (p.ExitCode != 0)
            throw new Exception($"Command failed: {file} {string.Join(" ", args)}\n{stderr}");
        if (!string.IsNullOrWhiteSpace(stdout))
            AnsiConsole.WriteLine(stdout.Trim());
    }

    public async Task<bool> Exists(string cmd)
    {
        try { await Run(cmd, "--version"); return true; } catch { return false; }
    }

    private static string Escape(string s) =>
        s.Contains(' ') ? $"\"{s}\"" : s;
}
