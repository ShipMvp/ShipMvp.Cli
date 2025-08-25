using System.CommandLine;
using Spectre.Console;
using ShipMvp.Cli.src.Services;

namespace ShipMvp.Cli.src.Commands;

public static class DoctorCommand
{
    public static Command Build()
    {
        var cmd = new Command("doctor", "Check prerequisites");
        cmd.SetHandler(async () =>
        {
            var sh = new Shell();
            await Report(sh, "git");
            await Report(sh, "dotnet");
            await Report(sh, "node");
            await Report(sh, "pnpm");
            await Report(sh, "docker");
            AnsiConsole.MarkupLine("[green]Done.[/]");
        });
        return cmd;
    }

    static async Task Report(Shell sh, string name)
    {
        var ok = await sh.Exists(name);
        AnsiConsole.MarkupLine(ok ? $"[green]✓ {name}[/]" : $"[red]✗ {name}[/]");
    }
}
