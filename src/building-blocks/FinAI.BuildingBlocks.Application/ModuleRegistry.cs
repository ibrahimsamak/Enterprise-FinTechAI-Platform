using System.Reflection;

public static class ModuleRegistry
{
    public static readonly Assembly[] Assemblies =
        AppDomain.CurrentDomain.GetAssemblies()
            .Concat(LoadReferencedModuleAssemblies())
            .Where(a => a.GetName().Name?.StartsWith("FinAI.Modules.") == true)
            .Distinct()
            .ToArray();

    public static IReadOnlyList<IModule> LoadModules() =>
        Assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IModule).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false })
            .Select(t => (IModule)Activator.CreateInstance(t)!)
            .ToList();

    private static IEnumerable<Assembly> LoadReferencedModuleAssemblies()
    {
        var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        return Directory.GetFiles(dir, "FinAI.Modules.*.dll")
            .Select(p => Assembly.LoadFrom(p));
    }
}
