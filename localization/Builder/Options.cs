using CommandLine;

namespace Mews.LocalizationBuilder
{
    public sealed class Options
    {
        [Option(nameof(SourceLanguage), Required = true, HelpText = "Source language code.")]
        public string SourceLanguage { get; set; }

        [Option(nameof(RepositoryOwner), Required = true, HelpText = "Git repository owner.")]
        public string RepositoryOwner { get; set; }

        [Option(nameof(Repository), Required = true, HelpText = "Git repository name.")]
        public string Repository { get; set; }

        [Option(nameof(RepositoryAccessToken), Required = true, HelpText = "Git repository access token.")]
        public string RepositoryAccessToken { get; set; }

        [Option(nameof(StorageContainerUri), Required = true, HelpText = "Azure Blob Storage container URI.")]
        public string StorageContainerUri { get; set; }

        [Option(nameof(ServicePrincipalTenantId), Required = true, HelpText = "Tenant ID of the service principal to use for accessing Azure Storage.")]
        public string ServicePrincipalTenantId { get; set; }

        [Option(nameof(ServicePrincipalClientId), Required = true, HelpText = "Client ID of the service principal to use for accessing Azure Storage.")]
        public string ServicePrincipalClientId { get; set; }

        [Option(nameof(ServicePrincipalClientSecret), Required = true, HelpText = "Client secret used to authenticate the service principal.")]
        public string ServicePrincipalClientSecret { get; set; }

        [Option(nameof(Commit), Required = true, HelpText = "ID of the commit to be published.")]
        public string Commit { get; set; }

        [Option(nameof(DataDirectory), Required = true, HelpText = "Directory where per-language subdirectories with translation data are located.")]
        public string DataDirectory { get; set; }
    }
}