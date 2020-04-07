using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FuncSharp;
using Octokit;

namespace Mews.LocalizationBuilder.Helper
{
    public static class GitHelper
    {
        private static Regex KeyChangePattern => new Regex(@"^(-|\+)\s*""([a-zA-Z]+)""");

        public static IEnumerable<string> GetDeletions(string diffPatch)
        {
            return GetKeyModifications(diffPatch).Where(m => m.Change.Equals(KeyChange.Deletion)).Select(m => m.Key);
        }

        private static IEnumerable<(string Key, KeyChange Change)> GetKeyModifications(string diffPatch)
        {
            var modifications = diffPatch.Split(Environment.NewLine).Select(line =>
            {
                var match = KeyChangePattern.Match(line);
                return match.Success.Match(
                    t => (
                        Key: match.Groups[2].Value,
                        Change: match.Groups[1].Value.Match(
                            "-", _ => KeyChange.Deletion,
                            "+", _ => KeyChange.Addition
                        )
                    ).ToOption(),
                    f => Option.Empty
                );
            }).Flatten();

            return modifications.GroupBy(m => m.Key).Select(g => (
                Key: g.Key,
                Change: GetActualChange(g.Select(m => m.Change))
            ));
        }

        private static KeyChange GetActualChange(IEnumerable<KeyChange> changes)
        {
            if (changes.Contains(KeyChange.Addition) && changes.Contains(KeyChange.Deletion))
            {
                return KeyChange.Modification;
            }

            return changes.Single();
        }
    }
}