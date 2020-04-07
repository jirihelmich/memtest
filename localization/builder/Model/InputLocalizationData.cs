using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FuncSharp;

namespace Mews.LocalizationBuilder.Model
{
    public sealed class InputLocalizationData : Dictionary<string, InputLanguageData>
    {
        private static Regex PlaceholderRegex => new Regex(@"\{([a-zA-Z][a-zA-Z0-9]*)\}", RegexOptions.Compiled);

        public static InputLocalizationData Read(string valuePath, string sourceLanguage)
        {
            var data = new InputLocalizationData();
            foreach (var directoryPath in Directory.GetDirectories(valuePath))
            {
                var directoryName = Path.GetFileName(directoryPath);
                if (Regex.IsMatch(directoryName, "[a-z]{2}_[A-Z]{2}"))
                {
                    var languageCode = directoryName.Replace("_", "-");
                    var filePaths = Directory.GetFiles(directoryPath, "*.resjson");

                    foreach (var filePath in filePaths)
                    {
                        try
                        {
                            var values = InputLanguageData.FromJson(File.ReadAllText(filePath), includeMetadata: languageCode.Equals(sourceLanguage, StringComparison.InvariantCultureIgnoreCase));

                            if (data.ContainsKey(languageCode))
                            {
                                foreach (var cleanValue in values)
                                {
                                    data[languageCode][cleanValue.Key] = cleanValue.Value;
                                }
                            }
                            else
                            {
                                data.Add(languageCode, values);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error when parsing localization file {filePath}: {e}");
                            throw;
                        }
                    }
                }
            }

            return data;
        }

        public Dto.LocalizationData Serialize(string defaultLanguageCode)
        {
            var defaultLanguage = this[defaultLanguageCode];

            return new Dto.LocalizationData
            {
                Keys = defaultLanguage.ToDictionary(
                    p => p.Key,
                    p => new Dto.Key
                    {
                        Comment = p.Value.Comment.GetOrNull(),
                        Scopes = p.Value.Scopes,
                        Parameters = GetParameters(p.Value.Text)
                    }
                ),
                Values = this.ToDictionary(
                    p => p.Key,
                    p => p.Value.ToDictionary(
                        kp => kp.Key,
                        kp => kp.Value.Text
                    )
                )
            };
        }

        private static IReadOnlyCollection<string> GetParameters(string text)
        {
            return PlaceholderRegex.Matches(text).Select(m => m.Groups[1].Value).ToList();
        }
    }
}