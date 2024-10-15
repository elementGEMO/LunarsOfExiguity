/*
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx.Configuration;

using static LunarsOfExiguity.ColorCode;

namespace LunarsOfExiguity
{
    public class PurifiedDrowned : ItemBase
    {
        protected override string Name => "PurifiedDrowned";
        protected override string Token => "PURIFIED_GESTUREOFTHEDROWNED";
        protected override bool CanRemove => false;
        protected override ItemTierDef ModdedTier => PurifiedTier.PurifiedTierDef;
        protected override ItemTag[] ItemTags => [ItemTag.CannotCopy | ItemTag.CannotDuplicate];
        protected override Sprite PickupIconSprite => AssetStatics.bundle.LoadAsset<Sprite>("PureGestureIcon");
        protected override GameObject PickupModelPrefab => AssetStatics.bundle.LoadAsset<GameObject>("PurifiedGesturePrefab");
        protected override void LanguageTokens()
        {
            LanguageAPI.Add(ItemDef.nameToken, "Clarity of the Drowned");
            //LanguageAPI.Add(ItemDef.pickupToken, "Your " + "Lunar ".Style(FontColor.cIsLunar) + "items have shattered into pieces.");
            //LanguageAPI.Add(ItemDef.descriptionToken, "The aftermath of obtaining additional " + "Lunar ".Style(FontColor.cIsLunar) + "items.");
        }
        protected override void Methods()
        {
            PurifiedTier.ItemPurifiedPool.Add(ItemDef.itemIndex);
            PurifiedTier.AvailableTierDropList.Add(PickupDef.pickupIndex);
            //HG.ArrayUtils.ArrayAppend(ref PurifiedTier.ItemPurifiedPool, ItemDef.itemIndex);
            //HG.ArrayUtils.ArrayAppend(ref PurifiedTier.AvailableTierDropList, ItemDef.CreatePickupDef());
            //PurifiedTier.ItemPurifiedPool
            //Inventory.onServerItemGiven += StopLunarStacking;
        }
    }
}
*/
