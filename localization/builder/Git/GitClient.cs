using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public Task<GitHubCommit> GetCommit(string commitHash)
        {
            return Client.Repository.Commit.Get(RepositoryOwner, Repository, commitHash);
        }

        public Task<IReadOnlyList<GitHubCommit>> GetCommits(string firstCommitHash, string lastCommitHash)
        {
            var comparisonResult = Client.Repository.Commit.Compare(RepositoryOwner, Repository, firstCommitHash, lastCommitHash);
            return comparisonResult.Map(r => r.Commits);
        }

        private string RepositoryOwner { get; }

        private string Repository { get; }

        private GitHubClient Client { get; }
    }
}