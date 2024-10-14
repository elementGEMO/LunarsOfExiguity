using R2API;
using RoR2;
using System;
using UnityEngine;

namespace LunarsOfExiguity
{
    public abstract class ItemBase
    {
        protected abstract string Name { get; }
        protected abstract string Token { get; }
        protected virtual bool CanRemove => false;
        protected virtual bool IsHidden => false;
        protected virtual ItemTag[] ItemTags => Array.Empty<ItemTag>();
        protected virtual ItemTier ItemTier => ItemTier.NoTier;
        protected virtual ItemTierDef ModdedTier => null;
        protected virtual GameObject PickupModelPrefab => null;
        protected virtual Sprite PickupIconSprite => null;
        public ItemDef ItemDef;
        public ItemBase() => Initialize();

        private void Initialize(bool enabled = true)
        {
            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ItemDef.name = Name;
            ItemDef.canRemove = CanRemove;
            ItemDef.hidden = IsHidden;
            ItemDef.tags = ItemTags;
            ItemDef.pickupModelPrefab = PickupModelPrefab;
            ItemDef.pickupIconSprite = PickupIconSprite;

            if (!ModdedTier) ItemDef.deprecatedTier = ItemTier;
            else ItemDef._itemTierDef = ModdedTier;

            ItemDef.AutoPopulateTokens();
            ItemDef.nameToken = AssetStatics.tokenPrefix + Token + "_NAME";
            ItemDef.pickupToken = AssetStatics.tokenPrefix + Token + "_PICKUP";
            ItemDef.descriptionToken = AssetStatics.tokenPrefix + Token + "_DESC";
            ItemDef.loreToken = AssetStatics.tokenPrefix + Token + "_LORE";

            ItemAPI.Add(new CustomItem(ItemDef, []));

            LanguageTokens();
            Methods();
        }
        protected virtual void LanguageTokens() { }
        protected virtual void Methods() { }
    }
    public abstract class ItemBaseRework
    {
        protected virtual string Token => null;
        public ItemBaseRework(bool configValue = true) => Initialize(configValue);

        private void Initialize(bool configValue)
        {
            if (configValue) {
                LanguageTokens();
                Methods();
            }
            else
            {
                DisabledTokens();
            }
        }
        protected virtual void LanguageTokens() { }
        protected virtual void DisabledTokens() { }
        protected virtual void Methods() { }
    }
    public static class ItemUtils
    {
        public static string SignVal(this float value) => value >= 0 ? "+" + value : "-" + value;
        public static float RoundVal(float value) => MathF.Round(value, MainConfig.RoundNumber.Value);
    }
}
