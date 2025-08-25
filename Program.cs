using System.CommandLine;
using ShipMvp.Cli.src.Commands;

var root = new RootCommand("ShipMVP CLI");

root.AddCommand(NewCommand.Build());
root.AddCommand(ModuleAddCommand.Build());
root.AddCommand(MigrateAddCommand.Build());
root.AddCommand(UpdateCommand.Build());
root.AddCommand(DoctorCommand.Build());

return await root.InvokeAsync(args);
