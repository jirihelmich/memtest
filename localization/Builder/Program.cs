using System;
using System.Linq;
using CommandLine;
using FuncSharp;
using Mews.LocalizationBuilder.Model;
using Mews.LocalizationBuilder.Storage;
using Mews.LocalizationBuilder.Validation;
using Mews.Time;

namespace Mews.LocalizationBuilder
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                var result = Run(options);
                result.Error.Match(validationErrors => throw new Exception(validationErrors.Select(e => e.Text).MkLines()));
            });
        }

        private static ITry<Unit, INonEmptyEnumerable<Validation.Error>> Run(Options options)
        {
            var storageClient = new StorageClient(
                containerUri: new Uri(options.StorageContainerUri),
                tenantId: options.ServicePrincipalTenantId,
                clientId: options.ServicePrincipalClientId,
                clientSecret: options.ServicePrincipalClientSecret
            );

            var version = GenerateFreshVersion(StaticDateTimeProvider.NowUtc);
            var localData = InputLocalizationData.Read(options.DataDirectory, options.SourceLanguage);
            var currentData = storageClient.ReadCurrentVersion();
            var errors = currentData.FlatMap(data => Validator.Validate(localData, data, options.SourceLanguage, options.AllowKeyRemoval).AsNonEmpty());

            if (errors.IsEmpty)
            {
                var updatedData = new VersionedLocalizationData(
                    versionData: new VersionData(version, options.Commit),
                    localization: localData.Serialize(options.SourceLanguage)
                );

                storageClient.Upload(updatedData);
                storageClient.Update(new Manifest(version, version));
            }

            return errors.Match(
                Try.Error<Unit, INonEmptyEnumerable<Validation.Error>>,
                Try.Success<Unit, INonEmptyEnumerable<Validation.Error>>
            );
        }

        private static Version GenerateFreshVersion(DateTime dateTime)
        {
            return new Version($"{dateTime.Year}{dateTime.Month:D2}{dateTime.Day:D2}.{dateTime.Hour}.{dateTime.Minute}.{dateTime.Second}");
        }
    }
}