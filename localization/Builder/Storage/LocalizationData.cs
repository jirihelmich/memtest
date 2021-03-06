﻿using System.Collections.Generic;

namespace Mews.LocalizationBuilder.Storage
{
    public sealed class LocalizationData
    {
        public Dictionary<string, Key> Keys { get; set; }

        public Dictionary<string, Dictionary<string, string>> Values { get; set; }
    }
}