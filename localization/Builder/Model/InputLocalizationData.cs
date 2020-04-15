using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FuncSharp;
using Mews.Linguistics;

namespace Mews.LocalizationBuilder.Model
{
    public sealed class InputLocalizationData
    {
        private InputLocalizationData(IDictionary<Language, Translation> data)
        {
            Data = new Dictionary<Language, Translation>(data);
        }
        public Dictionary<Language, Translation> Data { get; }

        private static Regex PlaceholderRegex => new Regex(@"\{([a-zA-Z][a-zA-Z0-9]*)\}", RegexOptions.Compiled);

        public static InputLocalizationData Read(string valuePath, string sourceLanguage)
        {
            var languageDirectories = GetLanguageDirectories(valuePath);
            var data = languageDirectories.GroupBy(d => d.Language).ToDictionary(
                g => g.Key,
                g => ReadLanguageData(g.SelectMany(t => Directory.GetFiles(t.Path, "*.resjson")), includeMetadata: g.Key.Code.SafeEquals(sourceLanguage))
            );

            return new InputLocalizationData(data);
        }

        public Dto.LocalizationData Serialize(string defaultLanguageCode)
        {
            var defaultLanguage = Data[Languages.GetByCode(defaultLanguageCode).Get()];

            return new Dto.LocalizationData
            {
                Keys = defaultLanguage.Data.ToDictionary(
                    p => p.Key,
                    p => new Dto.Key
                    {
                        Comment = p.Value.Metadata.Map(d => d.Comment).GetOrNull(),
                        Scopes = p.Value.Metadata.Map(d => d.Scopes).GetOrElse(KeyScopes.None),
                        Parameters = GetParameters(p.Value.Text)
                    }
                ),
                Values = Data.ToDictionary(
                    p => p.Key.Code,
                    p => p.Value.Data.ToDictionary(
                        kp => kp.Key,
                        kp => kp.Value.Text
                    )
                )
            };
        }

        private static Translation ReadLanguageData(IEnumerable<string> filePaths, bool includeMetadata)
        {
            var languageData = filePaths.Select(p => Translation.FromJson(File.ReadAllText(p), includeMetadata: includeMetadata));
            var combined = languageData.Aggregate(Translation.Combine);

            return combined;
        }

        private static IEnumerable<(Language Language, string Path)> GetLanguageDirectories(string basePath)
        {
            var directories = Directory.GetDirectories(basePath).Select(p => (FullPath: p, DirectoryName: Path.GetFileName(p)));
            return directories.Where(d => Regex.IsMatch(d.DirectoryName, "[a-z]{2}_[A-Z]{2}")).Select(d => (
                Language: Languages.GetByCode(d.DirectoryName.Replace("_", "-")).Get(),
                Path: d.FullPath
            ));
        }

        private static IStrictEnumerable<string> GetParameters(string text)
        {
            return PlaceholderRegex.Matches(text).Select(m => m.Groups[1].Value).AsStrict();
        }
    }
}