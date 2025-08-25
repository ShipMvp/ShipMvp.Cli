namespace ShipMvp.Cli.src.Services;

public sealed class Fs
{
    private readonly Stack<string> _cwd = new();

    public void EnsureEmptyDir(string path)
    {
        if (Directory.Exists(path)) throw new Exception($"Directory already exists: {path}");
        Directory.CreateDirectory(path);
    }

    public void Mkdir(string rel) => Directory.CreateDirectory(rel);

    public void Cd(string rel)
    {
        _cwd.Push(Directory.GetCurrentDirectory());
        Directory.SetCurrentDirectory(Path.GetFullPath(rel));
    }

    public void Popd()
    {
        if (_cwd.Count > 0) Directory.SetCurrentDirectory(_cwd.Pop());
    }

    public void WriteText(string path, string content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, content);
    }

    public void CopyFromTemplate(string templatePath, string targetPath, Dictionary<string,string> tokens)
    {
        var src = File.ReadAllText(templatePath);
        foreach (var kv in tokens)
            src = src.Replace("{{" + kv.Key + "}}", kv.Value);
        WriteText(targetPath, src);
    }

    public string OutPath(params string[] segments) => Path.GetFullPath(Path.Combine(segments));
    public bool FileExists(string path) => File.Exists(path);
}
