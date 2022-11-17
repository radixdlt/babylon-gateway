using System;

namespace RadixDlt.CoreApiSdk.Model;

public partial class LedgerTransaction
{
    public string PayloadHex
    {
        get
        {
            switch (ActualInstance)
            {
                case UserLedgerTransaction ult:
                    return ult.PayloadHex;
                case ValidatorLedgerTransaction vlt:
                    return vlt.PayloadHex;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ActualInstance), ActualInstance, null);
            }
        }
    }
}
