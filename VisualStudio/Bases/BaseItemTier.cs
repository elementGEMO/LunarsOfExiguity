using R2API;
using RoR2;
using UnityEngine;

namespace LunarsOfExiguity;

public abstract class BaseItemTier
{
    protected abstract string Name { get; }

    protected virtual ColorCatalog.ColorIndex Color => ColorCatalog.ColorIndex.None;
    protected virtual ColorCatalog.ColorIndex DarkColor => ColorCatalog.ColorIndex.None;

    protected virtual bool CanBeDropped => false;
    protected virtual bool CanBeScrapped => false;
    protected virtual bool CanBeRestacked => true;

    protected virtual ItemTierDef.PickupRules PickupRules => ItemTierDef.PickupRules.Default;

    public BaseItemTier() => Create();

    public ItemTierDef Get() => _itemTierDef;
    
    private void Create()
    {
        _itemTierDef = ScriptableObject.CreateInstance<ItemTierDef>();
        _itemTierDef.name = Name;

        _itemTierDef.colorIndex = Color;
        _itemTierDef.darkColorIndex = DarkColor;
        
        _itemTierDef.isDroppable = CanBeDropped;
        _itemTierDef.canScrap = CanBeScrapped;
        _itemTierDef.canRestack = CanBeRestacked;

        _itemTierDef.pickupRules = PickupRules;
    }


    private ItemTierDef _itemTierDef;
}