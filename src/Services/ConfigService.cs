using System.Text.Json;
using ShipMvp.Cli.src.Models;

namespace ShipMvp.Cli.src.Services;

public sealed class ConfigService
{
    private const string FileName = ".shipmvp.json";

    public ToolConfig LoadOrCreate(string root, ToolConfig defaults)
    {
        var path = Path.Combine(root, FileName);
        if (!File.Exists(path))
        {
            Save(root, defaults);
            return defaults;
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ToolConfig>(json) ?? defaults;
    }

    public void Save(string root, ToolConfig cfg)
    {
        var path = Path.Combine(root, FileName);
        var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}
