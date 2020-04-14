using System.Collections.Generic;
using System.Linq;
using Mews.LocalizationBuilder.Extensions;
using Octokit;

namespace Mews.LocalizationBuilder.Git
{
    public sealed class GitClient
    {
        public GitClient(string repositoryOwner, string repository, string accessToken)
        {
            RepositoryOwner = repositoryOwner;
            Repository = repository;
            Client = new GitHubClient(new ProductHeaderValue("devops-pipeline"));
            Client.Credentials = new Credentials(accessToken);
        }

        private string RepositoryOwner { get; }

        private string Repository { get; }

        private GitHubClient Client { get; }

        public GitHubCommit GetCommit(string commitHash)
        {
            return Client.Repository.Commit.Get(RepositoryOwner, Repository, commitHash).Result();
        }

        public IReadOnlyList<GitHubCommit> GetCommits(string firstCommitHash, string lastCommitHash)
        {
            var comparisonResult = Client.Repository.Commit.Compare(RepositoryOwner, Repository, firstCommitHash, lastCommitHash).Result();
            return comparisonResult.Commits.Select(c => GetCommit(c.Sha)).ToList();
        }
    }
}