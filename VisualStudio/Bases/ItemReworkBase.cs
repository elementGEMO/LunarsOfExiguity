using RoR2;

namespace LunarsOfExiguity;

public abstract class ItemReworkBase : GenericBase<ItemDef>
{
    protected virtual string DisplayNameOverride { get; }
    protected virtual string PickupOverride { get; }
    protected virtual string DescriptionOverride { get; }
    protected virtual string LoreOverride { get; }

    protected virtual bool IsNameChangeEnabled() => true;
}