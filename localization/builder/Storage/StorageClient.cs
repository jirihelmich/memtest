using System;
using System.IO;
using System.Text;
using Azure.Identity;
using Azure.Storage.Blobs;
using FuncSharp;
using Newtonsoft.Json;

namespace Mews.LocalizationBuilder.Storage
{
    public sealed class StorageClient
    {
        public StorageClient(Uri containerUri, string tenantId, string clientId, string clientSecret)
        {
            Client = new BlobContainerClient(containerUri, new ClientSecretCredential(tenantId, clientId, clientSecret));
        }

        private BlobContainerClient Client { get; }

        public Option<Dto.VersionedLocalizationData> ReadCurrentVersion()
        {
            var manifest = ReadManifest();
            return manifest.FlatMap(m => Read(m.Current));
        }

        public void Upload(Dto.VersionedLocalizationData data)
        {
            Upload(BlobPath(new Version(data.VersionData.Version)), JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented), overwrite: false);
        }

        public void Update(Manifest manifest)
        {
            Upload("manifest.json", manifest.ToJson(), overwrite: true);
        }

        private static string BlobPath(Version version)
        {
            return $"{version}/data.json";
        }

        private Option<Manifest> ReadManifest()
        {
            return Read<Manifest>("manifest.json");
        }

        private Option<Dto.VersionedLocalizationData> Read(Version version)
        {
            return Read<Dto.VersionedLocalizationData>(BlobPath(version));
        }

        private Option<T> Read<T>(string blobPath)
        {
            var blobClient = Client.GetBlobClient(blobPath);
            return Try.Create(_ =>
            {
                using var downloadInfo = blobClient.Download().Value;
                using var reader = new StreamReader(downloadInfo.Content, Encoding.UTF8);
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(json);
            }).Success;
        }

        private void Upload(string blobPath, string data, bool overwrite)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(data);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            var blobClient = Client.GetBlobClient(blobPath);
            blobClient.Upload(stream, overwrite);
        }
    }
}