using BepInEx.Configuration;
using RoR2;
using UnityEngine.Networking;

namespace LunarsOfExiguity.Content.Lunar.Items;

public class FracturedItem : ItemBase
{
    protected override string Name => "Fractured";

    protected override ItemTag[] Tags => [ItemTag.CannotCopy, ItemTag.CannotSteal, ItemTag.CannotDuplicate];

    protected override string DisplayName => "Fractured";
    protected override string PickupText => "Your " + "Lunar ".Style(ColorCode.FontColor.cIsLunar) + "items have shattered into pieces.";
    protected override string Description => "The aftermath of obtaining additional " + "Lunar ".Style(ColorCode.FontColor.cIsLunar) + "items.";

    protected override void Initialize()
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
            if (MainConfig.FracturedCount.Value == MainConfig.FracturedOptions.ItemOnFracture) inventory.GiveItem(Get().itemIndex, itemCount);
            CharacterMasterNotificationQueue.SendTransformNotification(masterComponent, itemIndex, Get().itemIndex, CharacterMasterNotificationQueue.TransformationType.LunarSun);
        }
    }

    public static ConfigEntry<bool> Gain_Item;
}