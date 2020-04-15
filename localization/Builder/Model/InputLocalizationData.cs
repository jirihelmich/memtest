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
                Keys = defaultLanguage.SerializeKeys(),
                Values = SerializeValues()
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

        private Dictionary<string, Dictionary<string, string>> SerializeValues()
        {
            return Data.ToDictionary(
                p => p.Key.Code,
                p => p.Value.SerializeTranslations()
            );
        }
    }
}