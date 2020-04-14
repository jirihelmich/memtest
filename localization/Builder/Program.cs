using System;
using System.Linq;
using CommandLine;
using FuncSharp;

namespace Mews.LocalizationBuilder
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                var result = Run(options);
                result.Error.Match(validationErrors => throw new Exception(validationErrors.Select(e => e.Text).MkLines()));
            });
        }

        private static ITry<Unit, IStrictEnumerable<Validation.Error>> Run(Options options)
        {
            return Try.Success<Unit, IStrictEnumerable<Validation.Error>>(Unit.Value);
        }
    }
}