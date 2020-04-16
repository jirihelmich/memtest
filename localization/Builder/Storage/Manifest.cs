using System;
using Mews.Json;

namespace Mews.LocalizationBuilder.Storage
{
    public sealed class Manifest
    {
        public Manifest(Version latest, Version current)
        {
            Latest = latest;
            Current = current;
        }

        public Version Latest { get; }

        public Version Current { get; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(escapeHtml: false, indent: true, value: new
            {
                Latest = Latest.ToString(),
                Current = Current.ToString()
            });
        }
    }
}