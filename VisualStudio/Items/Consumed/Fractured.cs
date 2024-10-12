using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;

using static LunarsOfExiguity.ColorCode;

namespace LunarsOfExiguity
{
    public class Fractured
    {
        public static ItemDef fracturedDef;
        public static readonly string token = "FRACTURED_";
        public Fractured()
        {
            LanguageAPI.Add(AssetStatics.tokenPrefix + token + "NAME", "Fractured");
            LanguageAPI.Add(AssetStatics.tokenPrefix + token + "PICKUP", "Your " + "Lunar ".Style(FontColor.cIsLunar) + "item has shattered into pieces.");
            LanguageAPI.Add(AssetStatics.tokenPrefix + token + "DESC", "The aftermath of obtaining additional " + "Lunar ".Style(FontColor.cIsLunar) + "items.");

            fracturedDef = ScriptableObject.CreateInstance<ItemDef>();
            fracturedDef.name = "FractureNoStack";
            fracturedDef.pickupIconSprite = AssetStatics.bundle.LoadAsset<Sprite>("IconLunarConsumed");
            fracturedDef.tags = [ItemTag.CannotCopy | ItemTag.CannotSteal | ItemTag.CannotDuplicate];
            fracturedDef.canRemove = false;
            fracturedDef.AutoPopulateTokens();
            fracturedDef.nameToken = AssetStatics.tokenPrefix + token + "NAME";
            fracturedDef.pickupToken = AssetStatics.tokenPrefix + token + "PICKUP";
            fracturedDef.descriptionToken = AssetStatics.tokenPrefix + token + "DESC";

            ItemAPI.Add(new CustomItem(fracturedDef, []));

            Inventory.onServerItemGiven += StopLunarStacking;

            Log.Debug(RoR2Content.Items.TonicAffliction.tier);
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
                inventory.GiveItem(fracturedDef.itemIndex, itemCount);
                CharacterMasterNotificationQueue.SendTransformNotification(masterComponent, itemIndex, fracturedDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.LunarSun);
            }
        }
    }
}
