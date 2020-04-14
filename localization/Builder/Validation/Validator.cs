using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FuncSharp;
using Mews.LocalizationBuilder.Git;
using Mews.LocalizationBuilder.Model;
using Octokit;

namespace Mews.LocalizationBuilder.Validation
{
    public sealed class Validator
    {
        public Validator(GitClient gitClient)
        {
            GitClient = gitClient;
        }

        private GitClient GitClient { get; }

        public IStrictEnumerable<Error> Validate(InputLocalizationData localData, Dto.VersionedLocalizationData storageData, string commitHash, string defaultLanguage)
        {
            var defaultLanguageLocalData = localData.Data.Single(p => p.Key.Code.SafeEquals(defaultLanguage)).Value;
            var errors = StrictEnumerable.CreateFlat(
                CheckKeyRemovals(defaultLanguageLocalData, storageData, commitHash, defaultLanguage)
            );

            return errors;
        }

        private static IOption<Error> CheckKeyRemoval(IEnumerable<GitHubCommit> commits, string key, string defaultLanguage)
        {
            Console.WriteLine($"Examining the following commits for removal of '{key}':\n{commits.Select(c => c.Sha).MkLines(prefix: "\t")}");
            var deletingCommit = commits.Last(c => c.Files.Any(f => IsDefaultLanguageFile(f.Filename, defaultLanguage) && Helper.Git.GetDeletions(f.Patch).Contains(key)));
            var isWrongCommitMessage = !deletingCommit.Commit.Message.StartsWith("Breaking");

            return isWrongCommitMessage.ToTrueOption().Map(_ => new Error($"Key '{key}' removed without indicating a breaking change."));
        }

        private static bool IsDefaultLanguageFile(string fileName, string defaultLanguage)
        {
            return Path.GetDirectoryName(fileName).EndsWith($"{Path.DirectorySeparatorChar}{defaultLanguage.Replace('-', '_')}");
        }

        private IStrictEnumerable<Error> CheckKeyRemovals(Translation localDefaultLanguageData, Dto.VersionedLocalizationData storageData, string commitHash, string defaultLanguage)
        {
            var missingKeys = storageData.Localization.Keys.Keys.Except(localDefaultLanguageData.Data.Keys);
            var commits = GitClient.GetCommits(storageData.VersionData.Commit, commitHash);
            var errors = missingKeys.Select(key => CheckKeyRemoval(commits, key, defaultLanguage)).Flatten();

            return errors.AsStrict();
        }
    }
}