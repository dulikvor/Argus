namespace Argus.Common.Data
{
    public static class Aliases
    {
        public delegate Task<(string Schema, string Token)> TokenCreator(string scope, string apiKey);
    }
}
