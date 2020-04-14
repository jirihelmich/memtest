using FuncSharp;

namespace Mews.LocalizationBuilder.Model
{
    public sealed class Key
    {
        public Key(string name, string text, KeyMetadata metaData = null)
        {
            Name = name;
            Text = text;
            Metadata = metaData.ToOption();
        }

        public static string MetaSuffix => ".meta";

        public string Name { get; }

        public string Text { get; }

        public IOption<KeyMetadata> Metadata { get; }
    }
}