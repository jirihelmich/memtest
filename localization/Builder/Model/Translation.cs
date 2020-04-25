using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FuncSharp;
using Newtonsoft.Json.Linq;

namespace Mews.LocalizationBuilder.Model
{
    public sealed class Translation
    {
        private Translation(IDictionary<string, Key> data)
        {
            Data = new Dictionary<string, Key>(data);
        }

        public Dictionary<string, Key> Data { get; }

        private static Regex KeyNameRegex => new Regex(@"^_?([a-zA-Z0-9]+)\.?", RegexOptions.Compiled);

        public static Translation Combine(Translation first, Translation second)
        {
            var combinedData = first.Data.Concat(second.Data).ToDictionary(p => p.Key, p => p.Value);
            return new Translation(combinedData);
        }

        public static Translation FromJson(string json, bool includeMetadata)
        {
            var jsonDocument = JObject.Parse(json);
            var keys = ParseKeys(jsonDocument, includeMetadata);
            var data = keys.ToDictionary(key => key.Name);

            return new Translation(data);
        }

        public Dictionary<string, Storage.Key> SerializeKeys()
        {
            return Data.ToDictionary(
                p => p.Key,
                p => new Storage.Key
                {
                    Comment = p.Value.Metadata.Map(d => d.Comment).GetOrNull(),
                    Scopes = p.Value.Metadata.Map(d => d.Scopes).GetOrElse(KeyScopes.None)
                }
            );
        }

        public Dictionary<string, string> SerializeTranslations()
        {
            return Data.ToDictionary(
                p => p.Key,
                p => p.Value.Text
            );
        }

        private static IStrictEnumerable<Key> ParseKeys(IDictionary<string, JToken> jsonDocument, bool includeMetadata)
        {
            var keyNames = jsonDocument.Keys.Distinct(KeyName);
            var keys = keyNames.Select(name => CreateKey(name, jsonDocument, includeMetadata));

            return StrictEnumerable.Create(keys);
        }

        private static Key CreateKey(string name, IDictionary<string, JToken> json, bool includeMetadata)
        {
            var text = json[name].Value<string>();
            var cleanText = text.Replace('\n', ' ').Trim();
            var metaKey = $"_{name}{Key.MetaSuffix}";
            var metadata = includeMetadata.ToTrueOption().FlatMap(_ => json.GetOrElse(metaKey).MapRef(t => t.ToObject<KeyMetadata>()).ToOption());

            return new Key(
                name,
                cleanText,
                metadata.GetOrNull()
            );
        }

        private static string KeyName(string jsonKey)
        {
            // Would be much nicer with a regex, but that is terribly slow.
            var isMetaKey = jsonKey.StartsWith("_");
            return isMetaKey.Match(
                t => jsonKey.Substring(1, jsonKey.IndexOf('.') - 1),
                f => jsonKey
            );
        }
    }
}