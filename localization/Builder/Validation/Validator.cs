using System.Linq;
using FuncSharp;
using Mews.LocalizationBuilder.Model;
using Mews.LocalizationBuilder.Storage;

namespace Mews.LocalizationBuilder.Validation
{
    public static class Validator
    {
        public static ITry<Unit, INonEmptyEnumerable<Error>> Validate(InputLocalizationData localData, VersionedLocalizationData storageData, string defaultLanguage)
        {
            var defaultLanguageLocalData = localData.Data.Single(p => p.Key.Code.SafeEquals(defaultLanguage)).Value;
            var errors = StrictEnumerable.CreateFlat(
                CheckKeyRemovals(defaultLanguageLocalData, storageData)
            );

            return errors.AsNonEmpty().Match(
                Try.Error<Unit, INonEmptyEnumerable<Error>>,
                Try.Success<Unit, INonEmptyEnumerable<Error>>
            );
        }

        private static IStrictEnumerable<Error> CheckKeyRemovals(Translation localDefaultLanguageData, VersionedLocalizationData storageData)
        {
            var missingKeys = storageData.Localization.Keys.Keys.Except(localDefaultLanguageData.Data.Keys);
            var errors = missingKeys.Select(key => new Error($"Key '{key}' has been removed."));

            return errors.AsStrict();
        }
    }
}