﻿namespace Mews.LocalizationBuilder.Storage
{
    public sealed class Key
    {
        public KeyScopes Scopes { get; set; }

        public IStrictEnumerable<string> Parameters { get; set; }

        public string Comment { get; set; }
    }
}