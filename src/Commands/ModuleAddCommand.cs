using System.CommandLine;
using ShipMvp.Cli.src.Services;
using ShipMvp.Cli.src.Models;

namespace ShipMvp.Cli.src.Commands;

public static class ModuleAddCommand
{
    public static Command Build()
    {
        var cmd = new Command("module", "Module operations");
        var add = new Command("add", "Add a backend (and optional frontend) module");
        var name = new Argument<string>("name", "Module name (e.g., Billing)");
        var fe = new Option<bool>("--fe", "Also scaffold frontend module");
        add.AddArgument(name); add.AddOption(fe);

        add.SetHandler(async (string n, bool scaffoldFe) =>
        {
            var sh = new Shell();
            var fs = new Fs();
            var cfgSvc = new ConfigService();
            var cfg = cfgSvc.LoadOrCreate(Directory.GetCurrentDirectory(), new ToolConfig());

            var sc = new Scaffolder(sh, fs, cfgSvc);
            await sc.AddModuleAsync(n, cfg, scaffoldFe);
        }, name, fe);

        cmd.Add(add);
        return cmd;
    }
}
