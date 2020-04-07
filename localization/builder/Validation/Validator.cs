using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FuncSharp;
using Mews.LocalizationBuilder.Helper;
using Mews.LocalizationBuilder.Extensions;
using Mews.LocalizationBuilder.Git;
using Mews.LocalizationBuilder.Model;
using Octokit;

namespace Mews.LocalizationBuilder.Validation
{
    public static class Validator
    {
        public static Task<IEnumerable<Error>> Validate(GitClient gitClient, InputLocalizationData localData, Dto.VersionedLocalizationData storageData, string commitHash, string defaultLanguage)
        {
            var defaultLanguageLocalData = localData.Single(p => p.Key.Equals(defaultLanguage)).Value;
            var checks = new[]
            {
                CheckKeyRemovals(gitClient, defaultLanguageLocalData, storageData, commitHash, defaultLanguage)
            };

            return Task.WhenAll(checks).Map(r => r.SelectMany(e => e));
        }

        private static Task<IEnumerable<Error>> CheckKeyRemovals(GitClient gitClient, InputLanguageData localDefaultLanguageData, Dto.VersionedLocalizationData storageData, string commitHash, string defaultLanguage)
        {
            var missingKeys = storageData.Localization.Keys.Keys.Except(localDefaultLanguageData.Keys);
            var errors = missingKeys.ToNonEmptyOption().FlatMap(
                keys =>
                {
                    var commits = gitClient.GetCommits(storageData.VersionData.Commit, commitHash);
                    return commits.FlatMap(c => Task.WhenAll(keys.Select(key => CheckKeyRemoval(gitClient, c, key, defaultLanguage))).Map(errs => errs.Flatten()));
                },
                _ => Enumerable.Empty<Error>()
            );

            return errors;
        }

        private static Task<Option<Error>> CheckKeyRemoval(GitClient gitClient, IEnumerable<GitHubCommit> commits, string key, string defaultLanguage)
        {
            Console.WriteLine($"Examining the following commits for removal of '{key}':\n{string.Join("\n", commits.Select(c => $"\t{c.Sha}"))}");
            var commitsWithFullInfo = commits.Select(c => gitClient.GetCommit(c.Sha)).ToList();
            return Task.WhenAll(commitsWithFullInfo).Map(cs =>
            {
                var deletingCommit = cs.Last(c => c.Files.Any(f => IsDefaultLanguageFile(f.Filename, defaultLanguage) && GitHelper.GetDeletions(f.Patch).Contains(key)));
                return deletingCommit.Commit.Message.StartsWith("Breaking").ToFalseOption().Map(_ => new Error($"Key '{key}' removed without indicating a breaking change."));
            });
        }

        private static bool IsDefaultLanguageFile(string fileName, string defaultLanguage)
        {
            return Path.GetDirectoryName(fileName).EndsWith($"{Path.DirectorySeparatorChar}{defaultLanguage.Replace('-', '_')}");
        }
    }
}