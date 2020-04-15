using System;
using Newtonsoft.Json;

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
            return JsonConvert.SerializeObject(new
            {
                Latest = Latest.ToString(),
                Current = Current.ToString()
            }, Formatting.Indented);
        }
    }
}