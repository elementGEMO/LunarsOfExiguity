using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LunarsOfExiguity;

public abstract class ItemTierBase : GenericBase<ItemTierDef>
{
    protected virtual ColorCatalog.ColorIndex Color => ColorCatalog.ColorIndex.None;
    protected virtual ColorCatalog.ColorIndex DarkColor => ColorCatalog.ColorIndex.None;

    protected virtual bool CanBeScrapped => false;
    protected virtual bool CanBeRestacked => true;

    protected virtual ItemTierDef.PickupRules PickupRules => ItemTierDef.PickupRules.Default;

    protected override void Create()
    {
        Value = ScriptableObject.CreateInstance<ItemTierDef>();

        if (Value)
        {
            Value.name = Name;

            Value.colorIndex = Color;
            Value.darkColorIndex = DarkColor;

            Value.highlightPrefab = HighlightPrefab;
            Value.dropletDisplayPrefab = DropletPrefab;

            Value.bgIconTexture = BackgroundTexture;

            Value.isDroppable = CanBeDropped;
            Value.canScrap = CanBeScrapped;
            Value.canRestack = CanBeRestacked;

            Value.pickupRules = PickupRules;

            Value.tier = ItemTier.AssignedAtRuntime;
            //Value._tier = ItemTier.AssignedAtRuntime;
        }

        ContentAddition.AddItemTierDef(Value);
    }
}