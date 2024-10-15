using RoR2;
using UnityEngine;

namespace LunarsOfExiguity;

public abstract class ItemTierBase : GenericBase<ItemTierDef>
{
    protected virtual ColorCatalog.ColorIndex Color => ColorCatalog.ColorIndex.None;
    protected virtual ColorCatalog.ColorIndex DarkColor => ColorCatalog.ColorIndex.None;

    protected virtual bool CanBeDropped => false;
    protected virtual bool CanBeScrapped => false;
    protected virtual bool CanBeRestacked => true;

    protected virtual ItemTierDef.PickupRules PickupRules => ItemTierDef.PickupRules.Default;

    protected override void Initialize()
    {
        Value = ScriptableObject.CreateInstance<ItemTierDef>();
        if (Value)
        {
            Value.name = Name;

            Value.colorIndex = Color;
            Value.darkColorIndex = DarkColor;
        
            Value.isDroppable = CanBeDropped;
            Value.canScrap = CanBeScrapped;
            Value.canRestack = CanBeRestacked;

            Value.pickupRules = PickupRules;
        }
    }
}