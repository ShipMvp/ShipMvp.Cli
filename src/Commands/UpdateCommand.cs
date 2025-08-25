using System.CommandLine;
using ShipMvp.Cli.src.Services;

namespace ShipMvp.Cli.src.Commands;

public static class UpdateCommand
{
    public static Command Build()
    {
        var cmd = new Command("update", "Update the shipmvp submodule");
        var target = new Argument<string>("ref", () => "stable", "Tag/branch to checkout in submodule");
        cmd.AddArgument(target);

        cmd.SetHandler(async (string @ref) =>
        {
            var sh = new Shell();
            await sh.Run("git", "-C", "shipmvp", "fetch", "--tags", "origin");
            await sh.Run("git", "-C", "shipmvp", "checkout", @ref);
            await sh.Run("git", "add", "shipmvp");
            await sh.Run("git", "commit", "-m", $"chore(shipmvp): bump to {@ref}");
        }, target);

        return cmd;
    }
}
