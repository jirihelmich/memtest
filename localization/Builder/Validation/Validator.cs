using System.Linq;
using Mews.LocalizationBuilder.Model;

namespace Mews.LocalizationBuilder.Validation
{
    public static class Validator
    {
        public static IStrictEnumerable<Error> Validate(InputLocalizationData localData, Dto.VersionedLocalizationData storageData, string commitHash, string defaultLanguage)
        {
            var defaultLanguageLocalData = localData.Data.Single(p => p.Key.Code.SafeEquals(defaultLanguage)).Value;
            var errors = StrictEnumerable.CreateFlat(
                CheckKeyRemovals(defaultLanguageLocalData, storageData, commitHash, defaultLanguage)
            );

            return errors;
        }

        private static IStrictEnumerable<Error> CheckKeyRemovals(Translation localDefaultLanguageData, Dto.VersionedLocalizationData storageData, string commitHash, string defaultLanguage)
        {
            var missingKeys = storageData.Localization.Keys.Keys.Except(localDefaultLanguageData.Data.Keys);
            var errors = missingKeys.Select(key => new Error($"Key '{key}' has been removed."));

            return errors.AsStrict();
        }
    }
}