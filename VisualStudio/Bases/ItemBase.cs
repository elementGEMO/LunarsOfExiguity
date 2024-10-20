using R2API;
using RoR2;
using UnityEngine;

namespace LunarsOfExiguity;
public abstract class ItemBase : GenericBase<ItemDef>
{
    protected virtual bool IsConsumed => false;
    protected virtual bool IsRemovable => false;
    protected virtual bool IsHidden => false;

    protected virtual GameObject PickupModelPrefab => null;
    protected virtual Sprite PickupIconSprite => null;

    protected virtual ItemTag[] Tags => [];
    protected virtual CombinedItemTier Tier => ItemTier.NoTier;

    protected virtual string DisplayName { get; }
    protected virtual string CursedNameOverride { get; }
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

        Value.pickupModelPrefab = PickupModelPrefab;
        Value.pickupIconSprite = PickupIconSprite;

        Value.tags = Tags;
        Value._itemTierDef = Tier;
        Value.deprecatedTier = Tier;

        if (Value)
        {
            Value.AutoPopulateTokens();
            Value.nameToken = LoEPlugin.TokenPrefix + Value.nameToken;
            Value.pickupToken = LoEPlugin.TokenPrefix + Value.pickupToken;
            Value.descriptionToken = LoEPlugin.TokenPrefix + Value.descriptionToken;
            Value.loreToken = LoEPlugin.TokenPrefix + Value.loreToken;

            if (LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Cursed)
            {
                if (!string.IsNullOrWhiteSpace(CursedNameOverride)) LanguageAPI.Add(Value.nameToken, CursedNameOverride);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(DisplayName)) LanguageAPI.Add(Value.nameToken, DisplayName);
            }

            if (!string.IsNullOrWhiteSpace(PickupText)) LanguageAPI.Add(Value.pickupToken, PickupText);
            if (!string.IsNullOrWhiteSpace(Description)) LanguageAPI.Add(Value.descriptionToken, Description);
            if (!string.IsNullOrWhiteSpace(Lore)) LanguageAPI.Add(Value.loreToken, Lore);

            CreateDisplay();
        }

        ItemAPI.Add(new CustomItem(Value, []));
    }
    protected virtual void CreateDisplay() { }
}