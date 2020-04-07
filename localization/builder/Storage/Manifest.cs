using System;
using Newtonsoft.Json;

namespace Mews.LocalizationBuilder.Storage
{
    public sealed class Manifest
    {
        public Version Latest { get; set; }

        public Version Current { get; set; }

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