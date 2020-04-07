using System;
using System.Collections.Generic;
using System.Linq;
using FuncSharp;
using Newtonsoft.Json.Linq;

namespace Mews.LocalizationBuilder.Model
{
    public sealed class InputLanguageData : Dictionary<string, LocalizationKeyData>
    {
        public static InputLanguageData FromJson(string json, bool includeMetadata)
        {
            var jsonDocument = JObject.Parse(json);
            var keys = GetKeys(jsonDocument, includeMetadata);
            var data = new InputLanguageData();

            foreach (var key in keys)
            {
                data[key.Name] = new LocalizationKeyData(
                    key.Text,
                    key.Metadata.FlatMap(d => d.Scopes.ToOption()).GetOrElse(_ => Enumerable.Empty<KeyScopes>()).Aggregate(KeyScopes.None, (a, s) => a | s),
                    key.Metadata.Map(d => d.Comment).GetOrNull()
                );
            }

            return data;
        }

        private static IEnumerable<Key> GetKeys(JObject jsonDocument, bool includeMetadata)
        {
            var enumerable = jsonDocument as IEnumerable<KeyValuePair<string, JToken>>;
            var byKeyName = enumerable.Where(p => includeMetadata || !p.Key.StartsWith("_")).GroupBy(p => KeyName(p.Key)).ToList();

            return byKeyName.Select(g =>
            {
                var name = g.Key;
                var text = g.SingleOrDefault(p => p.Value.Type.Equals(JTokenType.String)).Value.Value<string>();
                var cleanText = string.Join(" ", text.Split('\n')).Trim();
                var metadataToken = g.SingleOrDefault(p => p.Value.Type.Equals(JTokenType.Object)).ToOption().Where(v => !v.Equals(default(KeyValuePair<string, JToken>)));
                var meta = metadataToken.Map(p => p.Value.ToObject<KeyMetadata>());

                return new Key(
                    name,
                    cleanText,
                    meta.Map(m => new KeyMetadata(m.Comment, m.Scopes)).GetOrNull()
                );
            });
        }

        private static string KeyName(string jsonKey)
        {
            return jsonKey.EndsWith(Key.MetaSuffix).Match(
                t => jsonKey.Substring(0, jsonKey.IndexOf(Key.MetaSuffix, StringComparison.InvariantCulture)),
                f => jsonKey
            ).TrimStart('_');
        }

        private class KeyMetadata
        {
            public KeyMetadata(string comment, List<KeyScopes> scopes)
            {
                Comment = comment;
                Scopes = scopes;
            }

            public string Comment { get; }
            public List<KeyScopes> Scopes { get; }
        }

        private class Key
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

            public Option<KeyMetadata> Metadata { get; }
        }
    }
}