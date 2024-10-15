using R2API;
using RoR2;
using UnityEngine;

namespace LunarsOfExiguity;

public abstract class ItemBase : GenericBase<ItemDef>
{
    protected virtual bool IsConsumed => false;
    protected virtual bool IsRemovable => false;
    protected virtual bool IsHidden => false;
    
    protected virtual ItemTag[] Tags => [];
    protected virtual CombinedItemTier Tier => ItemTier.NoTier;
    
    protected virtual string DisplayName { get; }
    protected virtual string PickupText { get; }
    protected virtual string Description { get; }
    protected virtual string Lore { get; }

    protected override void Create()
    {
        Value = ScriptableObject.CreateInstance<ItemDef>();
        Value.name = Name;

        Value.isConsumed = IsConsumed;
        Value.canRemove = IsRemovable;
        Value.hidden = IsHidden;

        Value.tags = Tags;
        Value._itemTierDef = Tier;
        Value.deprecatedTier = Tier;
        
        if (Value)
        {
            Value.AutoPopulateTokens();
            Value.nameToken = AssetStatics.tokenPrefix + Value.nameToken;
            Value.pickupToken = AssetStatics.tokenPrefix + Value.pickupToken;
            Value.descriptionToken = AssetStatics.tokenPrefix + Value.descriptionToken;
            Value.loreToken = AssetStatics.tokenPrefix + Value.loreToken;
            
            if (!string.IsNullOrWhiteSpace(DisplayName)) LanguageAPI.Add(Value.nameToken, DisplayName);
            if (!string.IsNullOrWhiteSpace(PickupText)) LanguageAPI.Add(Value.pickupToken, PickupText);
            if (!string.IsNullOrWhiteSpace(Description)) LanguageAPI.Add(Value.descriptionToken, Description);
            if (!string.IsNullOrWhiteSpace(Lore)) LanguageAPI.Add(Value.loreToken, Lore);
        }
    }
}