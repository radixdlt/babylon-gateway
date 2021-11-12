namespace Common.Database.Models.Ledger.Substates;

/// <summary>
/// This is a marker class in the Hierachy, to distinguish Data substates from Balance substates.
/// Data substates should have an address and key - but the address will be stored on the relevant subclass, and the
/// key is typically implicit in the subclass name - so there aren't any fields on this class at present.
/// </summary>
public abstract class DataSubstateBase : SubstateBase
{
}
