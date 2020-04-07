using FuncSharp;

namespace Mews.LocalizationBuilder.Model
{
    public sealed class LocalizationKeyData
    {
        public LocalizationKeyData(string text, KeyScopes scopes, string comment = null)
        {
            Text = text;
            Scopes = scopes;
            Comment = comment.ToNonEmptyOption();
        }

        public string Text { get; }

        public KeyScopes Scopes { get; }

        public Option<string> Comment { get; }
    }
}