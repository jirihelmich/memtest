using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mews.LocalizationBuilder.Model
{
    public sealed class InputLocalizationData
    {
        private InputLocalizationData(IDictionary<string, Translation> data)
        {
            Data = new Dictionary<string, Translation>(data);
        }

        public Dictionary<string, Translation> Data { get; }

        public static InputLocalizationData Read(string valuePath, string sourceLanguage)
        {
            var languageDirectories = GetLanguageDirectories(valuePath);
            var data = languageDirectories.GroupBy(d => d.Language).ToDictionary(
                g => g.Key,
                g => ReadLanguageData(g.SelectMany(t => Directory.GetFiles(t.Path, "*.resjson")), includeMetadata: g.Key.SafeEquals(sourceLanguage))
            );

            return new InputLocalizationData(data);
        }

        private static Translation ReadLanguageData(IEnumerable<string> filePaths, bool includeMetadata)
        {
            var languageData = filePaths.Select(p => Translation.FromJson(File.ReadAllText(p), includeMetadata: includeMetadata));
            var combined = languageData.Aggregate(Translation.Combine);

            return combined;
        }

        private static IEnumerable<(string Language, string Path)> GetLanguageDirectories(string basePath)
        {
            var directories = Directory.GetDirectories(basePath).Select(p => (FullPath: p, DirectoryName: Path.GetFileName(p)));
            return directories.Where(d => Regex.IsMatch(d.DirectoryName, "[a-z]{2}_[A-Z]{2}")).Select(d => (
                Language: d.DirectoryName.Replace("_", "-"),
                Path: d.FullPath
            ));
        }
    }
}