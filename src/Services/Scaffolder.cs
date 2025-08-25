using Spectre.Console;
using ShipMvp.Cli.src.Models;

namespace ShipMvp.Cli.src.Services;

public sealed class Scaffolder
{
    private readonly Shell _sh; private readonly Fs _fs; private readonly ConfigService _cfg;

    public Scaffolder(Shell sh, Fs fs, ConfigService cfg) { _sh = sh; _fs = fs; _cfg = cfg; }

    public async Task NewAsync(ToolConfig o)
    {
        AnsiConsole.MarkupLine($"[green]Scaffolding {o.Name}...[/]");

        _fs.EnsureEmptyDir(o.Name);
        _fs.Cd(o.Name);

        await _sh.Run("git", "init");
        await _sh.Run("dotnet", "new", "sln", "-n", o.Name);

        // submodule
        await _sh.Run("git", "submodule", "add", "-b", o.ShipMvpBranch, o.ShipMvpRepo, "shipmvp");
        await _sh.Run("git", "submodule", "update", "--init", "--recursive");

        // dirs
        _fs.Mkdir("apps/backend");
        _fs.Mkdir("apps/frontend");
        _fs.Mkdir("modules");

        o.ApiProject = $"apps/backend/{o.Name}.Api";
        o.MigrationsProject = $"apps/backend/{o.Name}.Migrations";
        o.FrontendAppName = $"{o.Name.ToLowerInvariant()}-web";

        // host api
        await _sh.Run("dotnet", "new", "webapi", "-n", $"{o.Name}.Api", "-o", o.ApiProject);
        // migrations project
        await _sh.Run("dotnet", "new", "classlib", "-n", $"{o.Name}.Migrations", "-o", o.MigrationsProject);

        // add to sln
        await _sh.Run("dotnet", "sln", "add", $"{o.ApiProject}/{o.Name}.Api.csproj", $"{o.MigrationsProject}/{o.Name}.Migrations.csproj");

        // references
        await _sh.Run("dotnet", "add", $"{o.ApiProject}/{o.Name}.Api.csproj", "reference",
            "shipmvp/backend/src/ShipMvp.Infrastructure/ShipMvp.Infrastructure.csproj",
            "shipmvp/backend/src/ShipMvp.Modularity/ShipMvp.Modularity.csproj");

        // EF packages
        await _sh.Run("dotnet", "add", $"{o.ApiProject}/{o.Name}.Api.csproj", "package", "Npgsql.EntityFrameworkCore.PostgreSQL");
        await _sh.Run("dotnet", "add", $"{o.ApiProject}/{o.Name}.Api.csproj", "package", "Microsoft.EntityFrameworkCore.Design");
        await _sh.Run("dotnet", "add", $"{o.MigrationsProject}/{o.Name}.Migrations.csproj", "package", "Microsoft.EntityFrameworkCore.Design");
        await _sh.Run("dotnet", "add", $"{o.MigrationsProject}/{o.Name}.Migrations.csproj", "package", "Npgsql.EntityFrameworkCore.PostgreSQL");

        // Write API files from templates
        var tokens = new Dictionary<string,string> {
            ["Name"] = o.Name,
            ["MigrationsAssembly"] = $"{o.Name}.Migrations"
        };
        _fs.CopyFromTemplate(Templating.T("Backend/Api/Program.cs.hbs"), Path.Combine(o.ApiProject, "Program.cs"), tokens);
        _fs.CopyFromTemplate(Templating.T("Backend/Api/AppDbContextFactory.cs.hbs"), Path.Combine(o.ApiProject, "AppDbContextFactory.cs"), tokens);
        _fs.CopyFromTemplate(Templating.T("Backend/Api/appsettings.json.hbs"), Path.Combine(o.ApiProject, "appsettings.json"), tokens);
        _fs.CopyFromTemplate(Templating.T("Backend/Migrations/Migrations.csproj.hbs"), Path.Combine(o.MigrationsProject, $"{o.Name}.Migrations.csproj"), tokens);

        // Frontend: degit or git clone
        _fs.Cd("apps/frontend");
        if (await _sh.Exists("npx"))
            await _sh.Run("npx", "degit", $"{o.Org}/shipmvp-react", o.FrontendAppName);
        else
        {
            await _sh.Run("git", "clone", "--depth=1", $"{o.Org}/shipmvp-react", o.FrontendAppName);
            // remove nested .git
            var innerGit = Path.Combine(o.FrontendAppName, ".git");
            if (Directory.Exists(innerGit)) Directory.Delete(innerGit, true);
        }
        _fs.CopyFromTemplate(Templating.T("Frontend/env.example.hbs"),
            Path.Combine(o.FrontendAppName, ".env.local"),
            new() { ["VITE_API_BASE_URL"] = "http://localhost:5000" });

        _fs.Popd(); // back to repo root

        // Guard workflow
        _fs.WriteText(".github/workflows/guard-shipmvp.yml",
            File.ReadAllText(Templating.T("Github/guard-shipmvp.yml.hbs")));

        // Save config
        _cfg.Save(Directory.GetCurrentDirectory(), o);

        await _sh.Run("dotnet", "restore");
        await _sh.Run("git", "add", ".");
        await _sh.Run("git", "commit", "-m", $"chore: init {o.Name} with ShipMVP");

        AnsiConsole.MarkupLine("[green]Done.[/]");
    }

    public async Task AddModuleAsync(string name, ToolConfig o, bool frontend)
    {
        var modRoot = Path.Combine("modules", name);
        Directory.CreateDirectory(Path.Combine(modRoot, "Domain"));
        Directory.CreateDirectory(Path.Combine(modRoot, "Infrastructure"));

        // backend files
        var tok = new Dictionary<string,string>{{"Name", name},{"Schema", name.ToLowerInvariant()}};
        _fs.CopyFromTemplate(Templating.T("Backend/Module/Billing/Invoice.cs.hbs"), Path.Combine(modRoot, "Domain", "Invoice.cs"), tok);
        _fs.CopyFromTemplate(Templating.T("Backend/Module/Billing/InvoiceConfig.cs.hbs"), Path.Combine(modRoot, "Infrastructure", "InvoiceConfig.cs"), tok);
        _fs.CopyFromTemplate(Templating.T("Backend/Module/Billing/BillingModule.cs.hbs"), Path.Combine(modRoot, $"{name}Module.cs"), tok);

        await _sh.Run("dotnet", "sln", "add", $"{modRoot}/{name}.csproj");
        await _sh.Run("dotnet", "new", "classlib", "-n", name, "-o", modRoot);
        // add references
        await _sh.Run("dotnet", "add", $"{modRoot}/{name}.csproj", "reference",
            "shipmvp/backend/src/ShipMvp.Abstractions/ShipMvp.Abstractions.csproj");
        await _sh.Run("dotnet", "add", $"{o.ApiProject}/{o.Name}.Api.csproj", "reference", $"{modRoot}/{name}.csproj");

        if (frontend)
        {
            var feRoot = Path.Combine("apps/frontend", o.FrontendAppName, "src/modules", name.ToLowerInvariant());
            Directory.CreateDirectory(feRoot);
            _fs.CopyFromTemplate(Templating.T("Frontend/invoice.module.tsx.hbs"), Path.Combine(feRoot, "module.tsx"),
                new() { ["Name"] = name, ["Slug"] = name.ToLowerInvariant() });
        }

        AnsiConsole.MarkupLine($"[green]Module {name} created.[/]");
    }
}
