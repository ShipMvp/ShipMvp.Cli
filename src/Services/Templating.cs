namespace ShipMvp.Cli.src.Services;

public static class Templating
{
    public static string TemplatesRoot()
    {
        var exe = AppContext.BaseDirectory;
        return Path.Combine(exe, "Templates");
    }

    public static string T(string subpath) => Path.Combine(TemplatesRoot(), subpath);
}
