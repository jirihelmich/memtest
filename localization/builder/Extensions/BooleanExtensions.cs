using FuncSharp;

namespace Mews.LocalizationBuilder.Extensions
{
    public static class BooleanExtensions
    {
        public static Option<bool> ToFalseOption(this bool value)
        {
            return value.Match(t => Option.Empty, f => Option.False);
        }
    }
}