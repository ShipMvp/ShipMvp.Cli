using System.CommandLine;
using ShipMvp.Cli.src.Services;
using ShipMvp.Cli.src.Models;

namespace ShipMvp.Cli.src.Commands;

public static class MigrateAddCommand
{
    public static Command Build()
    {
        var cmd = new Command("migrate", "EF Core migration helpers");
        var add = new Command("add", "Add a migration");
        var name = new Argument<string>("name", "Migration name");
        add.AddArgument(name);

        add.SetHandler(async (string n) =>
        {
            var sh = new Shell();
            var cfg = new ConfigService().LoadOrCreate(Directory.GetCurrentDirectory(), new ToolConfig());
            await sh.Run("dotnet", "ef", "migrations", "add", n,
                "--project", $"{cfg.MigrationsProject}",
                "--startup-project", $"{cfg.ApiProject}",
                "--context", "AppDbContext");
        }, name);

        cmd.Add(add);
        return cmd;
    }
}
