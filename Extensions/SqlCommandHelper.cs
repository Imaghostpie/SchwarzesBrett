namespace SchwarzesBrett.Extensions
{
    public static class SqlCommandHelper
    {
        public static string GetCommand(this IConfiguration config, string cmdKey)
            => config.GetValue<string>($"SqlCommands:{cmdKey}") ?? throw new Exception();
    }
}
