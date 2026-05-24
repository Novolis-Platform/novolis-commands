using Novolis.Commands.Testing;

namespace Novolis.Commands.Engine.Tests.Support;

public static class TestContexts
{
    public static TestCommandContext Bridge { get; } = new()
    {
        ContextAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["pilot"] = "helm",
            ["conn"] = "helm",
            ["weaps"] = "tactical",
            ["guns"] = "tactical",
            ["tac"] = "tactical",
            ["eng"] = "engineering",
            ["damage"] = "engineering"
        }
    };

    public static TestCommandContext Empty { get; } = new();
}
