using System.Reflection;

[AttributeUsage(AttributeTargets.Class)]
public class TopicAttribute(string name) : Attribute
{
    public string Name => name;
}
public static class TopicResolver
{
    public static string For<T>() => For(typeof(T));
    public static string For(Type t) =>
       t.GetCustomAttribute<TopicAttribute>()?.Name
       ?? throw new InvalidOperationException($"{t.Name} is missing [Topic].");
}