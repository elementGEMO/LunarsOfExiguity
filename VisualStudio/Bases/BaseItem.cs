using RoR2;
using UnityEngine;

namespace LunarsOfExiguity;

public abstract class BaseItem
{
    protected abstract string Name { get; }
    
    protected virtual bool IsRemovable => false;
    protected virtual bool IsHidden => false;
    
    protected virtual ItemTag[] Tags => [];
    protected virtual CombinedItemTier Tier => ItemTier.NoTier;
    
    protected virtual string DisplayName { get; }
    protected abstract string PickupText { get; }
    protected abstract string Description { get; }
    protected abstract string Lore { get; }
    
    public BaseItem() => Create();

    private void Create()
    {
        _itemDef = ScriptableObject.CreateInstance<ItemDef>();
        _itemDef.name = Name;

        _itemDef.canRemove = IsRemovable;
        _itemDef.hidden = IsHidden;

        _itemDef.tags = Tags;
        _itemDef._itemTierDef = Tier;
        _itemDef.deprecatedTier = Tier;
        
        if (_itemDef)
        {
            _itemDef.AutoPopulateTokens();
            _itemDef.nameToken = AssetStatics.tokenPrefix + _itemDef.nameToken;
            _itemDef.pickupToken = AssetStatics.tokenPrefix + _itemDef.pickupToken;
            _itemDef.descriptionToken = AssetStatics.tokenPrefix + _itemDef.descriptionToken;
            _itemDef.loreToken = AssetStatics.tokenPrefix + _itemDef.loreToken;
        }
    }

    public ItemDef Get() => _itemDef;

    private ItemDef _itemDef;
}