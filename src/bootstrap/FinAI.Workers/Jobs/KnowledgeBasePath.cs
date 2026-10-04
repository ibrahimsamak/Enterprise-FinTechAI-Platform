namespace FinAI.Workers.Jobs;

public static class KnowledgeBasePath
{
    public static string Find(string path = "knowledge-base")
    {
        if (Path.IsPathRooted(path)) return path;
        for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, path);
            if (Directory.Exists(candidate)) return candidate;
        }
        return path;
    }
}
