using System.Collections.Generic;

namespace Mews.LocalizationBuilder.Dto
{
    public sealed class Key
    {
        public KeyScopes Scopes { get; set; }

        public IEnumerable<string> Parameters { get; set; }

        public string Comment { get; set; }
    }
}