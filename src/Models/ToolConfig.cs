namespace ShipMvp.Cli.src.Models;

public sealed class ToolConfig
{
    public string Name { get; set; } = "";
    public string Org { get; set; } = "your-org";
    public string ShipMvpRepo { get; set; } = "https://github.com/your-org/shipmvp";
    public string ShipMvpBranch { get; set; } = "stable";
    public string FrontendRepo { get; set; } = "https://github.com/your-org/shipmvp-react";
    public string PackageManager { get; set; } = "pnpm"; // pnpm/npm/yarn
    public string FrontendAppName { get; set; } = "";    // derived: {name}-web
    public string ApiProject { get; set; } = "";         // derived: apps/backend/{Name}.Api
    public string MigrationsProject { get; set; } = "";  // derived: apps/backend/{Name}.Migrations
}
