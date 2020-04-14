using FuncSharp;

namespace Mews.LocalizationBuilder.Validation
{
    public sealed class Error : Product1<string>
    {
        public Error(string text)
            : base(text)
        {
        }

        public string Text
        {
            get { return ProductValue1; }
        }
    }
}