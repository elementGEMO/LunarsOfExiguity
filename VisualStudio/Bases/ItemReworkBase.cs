using RoR2;

namespace LunarsOfExiguity;

public abstract class ItemReworkBase : GenericBase<ItemDef>
{
    protected virtual string RelicNameOverride { get; }
    protected virtual string CursedNameOverride { get; }
    
    protected virtual string PickupOverride { get; }
    protected virtual string DescriptionOverride { get; }
    protected virtual string LoreOverride { get; }
}