using System;
using System.Linq;
using System.Threading.Tasks;
using FuncSharp;
using CommandLine;
using Mews.LocalizationBuilder.Extensions;
using Mews.LocalizationBuilder.Git;
using Mews.LocalizationBuilder.Model;
using Mews.LocalizationBuilder.Storage;
using Mews.LocalizationBuilder.Validation;

namespace Mews.LocalizationBuilder
{
    public static class Program
    {
        private static Task Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args).MapResult(Run, _ => Task.FromResult(1));
        }

        private static Task Run(Options options)
        {
            var storageClient = new StorageClient(
                containerUri: new Uri(options.StorageContainerUri),
                tenantId: options.ServicePrincipalTenantId,
                clientId: options.ServicePrincipalClientId,
                clientSecret: options.ServicePrincipalClientSecret
            );

            var gitClient = new GitClient(
                repositoryOwner: options.RepositoryOwner,
                repository: options.Repository,
                accessToken: options.RepositoryAccessToken
            );

            var version = GetFreshVersion();
            var localData = InputLocalizationData.Read(options.DataDirectory, options.SourceLanguage);
            var currentData = storageClient.ReadCurrentVersion();
            var errors = currentData.FlatMap(data => Validator.Validate(gitClient, localData, data, options.Commit, options.SourceLanguage), _ => Enumerable.Empty<Validation.Error>());

            return errors.Map(e =>
            {
                e.ToNonEmptyOption().Match(errs => throw new Exception(string.Join("\n", errs)));

                var updatedData = new Dto.VersionedLocalizationData
                {
                    VersionData = new Dto.VersionData
                    {
                        Commit = options.Commit,
                        Version = version.ToString()
                    },
                    Localization = localData.Serialize(options.SourceLanguage)
                };

                storageClient.Upload(updatedData);
                storageClient.Update(new Manifest { Latest = version, Current = version });

                return 0;
            });
        }

        private static Version GetFreshVersion()
        {
            var now = DateTime.UtcNow;
            return new Version($"{now.Year}{now.Month:D2}{now.Day:D2}.{now.Hour}.{now.Minute}.{now.Second}");
        }
    }
}