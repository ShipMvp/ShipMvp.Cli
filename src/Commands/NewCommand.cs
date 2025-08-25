using System.CommandLine;
using Spectre.Console;
using ShipMvp.Cli.src.Models;
using ShipMvp.Cli.src.Services;

namespace ShipMvp.Cli.src.Commands;

public static class NewCommand
{
    public static Command Build()
    {
        var cmd = new Command("new", "Create a new ShipMVP project");
        var name = new Argument<string>("name", "Project name (solution/repo)");
        var org  = new Option<string>("--org", () => "your-org");
        var shipRepo = new Option<string>("--shipmvp-repo", () => "https://github.com/your-org/shipmvp");
        var shipBranch = new Option<string>("--shipmvp-branch", () => "stable");
        var feRepo = new Option<string>("--frontend-repo", () => "https://github.com/your-org/shipmvp-react");
        var pkg = new Option<string>("--pkg", () => "pnpm");

        cmd.AddArgument(name);
        cmd.AddOption(org);
        cmd.AddOption(shipRepo);
        cmd.AddOption(shipBranch);
        cmd.AddOption(feRepo);
        cmd.AddOption(pkg);

        cmd.SetHandler(async (string n, string o, string sr, string sb, string fr, string p) =>
        {
            var sh = new Shell();
            var fs = new Fs();
            var cfgSvc = new ConfigService();
            var sc = new Scaffolder(sh, fs, cfgSvc);

            // preflight
            await Require(sh, "git");
            await Require(sh, "dotnet");
            if (!await sh.Exists("node")) AnsiConsole.MarkupLine("[yellow]node not found (frontend still clones via git).[/]");
            if (!await sh.Exists("pnpm")) AnsiConsole.MarkupLine("[yellow]pnpm not found (use npm/yarn manually).[/]");

            var cfg = new ToolConfig {
                Name = n, Org = o, ShipMvpRepo = sr, ShipMvpBranch = sb, FrontendRepo = fr, PackageManager = p
            };
            await sc.NewAsync(cfg);
        }, name, org, shipRepo, shipBranch, feRepo, pkg);

        return cmd;
    }

    private static async Task Require(Shell sh, string cmd)
    {
        if (!await sh.Exists(cmd)) throw new Exception($"Required command missing: {cmd}");
    }
}
