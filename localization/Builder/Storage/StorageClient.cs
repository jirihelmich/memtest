using System;
using System.IO;
using System.Text;
using Azure.Identity;
using Azure.Storage.Blobs;
using FuncSharp;
using Mews.Json;

namespace Mews.LocalizationBuilder.Storage
{
    public sealed class StorageClient
    {
        public StorageClient(Uri containerUri, string tenantId, string clientId, string clientSecret)
        {
            Client = new BlobContainerClient(containerUri, new ClientSecretCredential(tenantId, clientId, clientSecret));
        }

        private BlobContainerClient Client { get; }

        public IOption<VersionedLocalizationData> ReadCurrentVersion()
        {
            var manifest = ReadManifest();
            return manifest.FlatMap(m => Read(m.Current));
        }

        public void Upload(VersionedLocalizationData data)
        {
            Upload(BlobPath(new Version(data.VersionData.Version)), JsonSerializer.Serialize(data, escapeHtml: false, indent: true), overwrite: false);
        }

        public void Update(Manifest manifest)
        {
            Upload("manifest.json", manifest.ToJson(), overwrite: true);
        }

        private static string BlobPath(Version version)
        {
            return $"{version}/data.json";
        }

        private IOption<Manifest> ReadManifest()
        {
            return Read<Manifest>("manifest.json");
        }

        private IOption<VersionedLocalizationData> Read(Version version)
        {
            return Read<VersionedLocalizationData>(BlobPath(version));
        }

        private IOption<T> Read<T>(string blobPath)
        {
            var blobClient = Client.GetBlobClient(blobPath);
            return Try.Create(_ =>
            {
                using (var downloadInfo = blobClient.Download().Value)
                using (var reader = new StreamReader(downloadInfo.Content, Encoding.UTF8))
                {
                    var json = reader.ReadToEnd();
                    return JsonSerializer.UnsafeDeserialize<T>(json);
                }
            }).Success;
        }

        private void Upload(string blobPath, string data, bool overwrite)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                var blobClient = Client.GetBlobClient(blobPath);
                blobClient.Upload(stream, overwrite);
            }
        }
    }
}