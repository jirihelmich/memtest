using System;

namespace Mews.LocalizationBuilder
{
    [Flags]
    public enum KeyScopes
    {
        None = 0,
        Commander = 1 << 0,
        Navigator = 1 << 1,
        Distributor = 1 << 2,
        Operator = 1 << 3,
        Mail = 1 << 4,
        Billing = 1 << 5
    }
}