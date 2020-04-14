using System.Collections.Generic;
using System.Linq;

namespace Mews.LocalizationBuilder.Model
{
    public sealed class KeyMetadata
    {
        public KeyMetadata(string comment, IEnumerable<KeyScopes> scopes = null)
        {
            Comment = comment;
            Scopes = scopes.Defined().Aggregate(KeyScopes.None, (a, s) => a | s);
        }

        public string Comment { get; }

        public KeyScopes Scopes { get; }
    }
}