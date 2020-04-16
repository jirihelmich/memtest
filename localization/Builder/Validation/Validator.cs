using System.Linq;
using FuncSharp;
using Mews.LocalizationBuilder.Model;
using Mews.LocalizationBuilder.Storage;

namespace Mews.LocalizationBuilder.Validation
{
    public static class Validator
    {
        public static IStrictEnumerable<Error> Validate(InputLocalizationData localData, VersionedLocalizationData storageData, string defaultLanguage)
        {
            var defaultLanguageLocalData = localData.Data.Single(p => p.Key.Code.SafeEquals(defaultLanguage)).Value;

            return CheckKeyRemovals(defaultLanguageLocalData, storageData);
        }

        private static IStrictEnumerable<Error> CheckKeyRemovals(Translation localDefaultLanguageData, VersionedLocalizationData storageData)
        {
            var missingKeys = storageData.Localization.Keys.Keys.Except(localDefaultLanguageData.Data.Keys);
            var errors = missingKeys.Select(key => new Error($"Key '{key}' has been removed."));

            return errors.AsStrict();
        }
    }
}