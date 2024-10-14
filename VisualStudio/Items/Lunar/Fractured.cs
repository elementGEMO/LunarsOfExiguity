using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx.Configuration;

using static LunarsOfExiguity.ColorCode;

namespace LunarsOfExiguity
{
    public class Fractured : ItemBase
    {
        protected override string Name => "FracturedNoStack";
        protected override string Token => "FRACTURED";
        protected override bool CanRemove => false;
        protected override ItemTier ItemTier => ItemTier.NoTier;
        protected override ItemTag[] ItemTags => [ItemTag.CannotCopy | ItemTag.CannotSteal | ItemTag.CannotDuplicate];
        protected override Sprite PickupIconSprite => AssetStatics.bundle.LoadAsset<Sprite>("IconLunarConsumed");
        protected override void LanguageTokens()
        {
            LanguageAPI.Add(ItemDef.nameToken, "Fractured");
            LanguageAPI.Add(ItemDef.pickupToken, "Your " + "Lunar ".Style(FontColor.cIsLunar) + "items have shattered into pieces.");
            LanguageAPI.Add(ItemDef.descriptionToken, "The aftermath of obtaining additional " + "Lunar ".Style(FontColor.cIsLunar) + "items.");
        }
        protected override void Methods()
        {
            Inventory.onServerItemGiven += StopLunarStacking;
        }
        private void StopLunarStacking(Inventory inventory, ItemIndex itemIndex, int itemCount)
        {
            if (!NetworkServer.active && inventory) return;

            ItemDef item = ItemCatalog.GetItemDef(itemIndex);
            CharacterMaster masterComponent = inventory.GetComponent<CharacterMaster>();

            if (!item || !masterComponent) return;

            if (item.tier == ItemTier.Lunar && inventory.GetItemCount(item) > 1)
            {
                itemCount = inventory.GetItemCount(item) - 1;
                inventory.RemoveItem(itemIndex, itemCount);
                if (MainConfig.FracturedCount.Value == MainConfig.FracturedOptions.ItemOnFracture) inventory.GiveItem(ItemDef.itemIndex, itemCount);
                CharacterMasterNotificationQueue.SendTransformNotification(masterComponent, itemIndex, ItemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.LunarSun);
            }
        }

        public static ConfigEntry<bool> Gain_Item;
    }
}
