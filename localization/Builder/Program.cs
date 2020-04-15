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

        private static ITry<Unit, IStrictEnumerable<Validation.Error>> Run(Options options)
        {
            var storageClient = new StorageClient(
                containerUri: new Uri(options.StorageContainerUri),
                tenantId: options.ServicePrincipalTenantId,
                clientId: options.ServicePrincipalClientId,
                clientSecret: options.ServicePrincipalClientSecret
            );

            var version = GetFreshVersion();
            var localData = InputLocalizationData.Read(options.DataDirectory, options.SourceLanguage);
            var currentData = storageClient.ReadCurrentVersion();
            var errors = currentData.FlatMap(data => Validator.Validate(localData, data, options.Commit, options.SourceLanguage).AsNonEmpty());

            return errors.Match(
                Try.Error<Unit, IStrictEnumerable<Validation.Error>>,
                _ =>
                {
                    var updatedData = new VersionedLocalizationData(
                        versionData: new VersionData(version, options.Commit),
                        localization: localData.Serialize(options.SourceLanguage)
                    );

                    storageClient.Upload(updatedData);
                    storageClient.Update(new Manifest(version, version));

                    return Try.Success<Unit, IStrictEnumerable<Validation.Error>>(Unit.Value);
                }
            );
        }

        private static Version GetFreshVersion()
        {
            var now = StaticDateTimeProvider.NowUtc;
            return new Version($"{now.Year}{now.Month:D2}{now.Day:D2}.{now.Hour}.{now.Minute}.{now.Second}");
        }
    }
}