using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx.Configuration;

public class FracturedItem : ItemBase
{
    public static ConfigEntry<float> Fracture_Delay;
    public static ConfigEntry<bool> Gain_Fracture;

    protected override string Name => "Fractured";
    public static ItemDef ItemDef;

    protected override GameObject PickupModelPrefab { get; }

    protected override ItemTag[] Tags => [ItemTag.CannotCopy, ItemTag.CannotSteal, ItemTag.CannotDuplicate];

    protected override string DisplayName => "Fractured";
    protected override string PickupText => "Your <style=cIsLunar>lunar</style items have shattered into pieces.";

    private void InitConfigs()
    {
        Fracture_Delay = LoEPlugin.Instance.Config.Bind(
            ItemDef.name + " - Item", "Fractured Delay", 0.5f,
            "[ 0.5 = 0.5s Fracture Notificaion Delay ]"
        );

        Gain_Fracture = LoEPlugin.Instance.Config.Bind(
            ItemDef.name + " - Item", "Gain Fractured Item", true,
            "[ True = Gain 'Fractured' when a Lunar is Fractured | False = Nothing Happens ]"
        );
    }

    protected override void Initialize()
    {
        ItemDef = Value;
        InitConfigs();

        Inventory.onInventoryChangedGlobal += OnInventoryChangedGlobal;
        RoR2Application.onFixedUpdate += OnFixedUpdate;
    }

    private void OnFixedUpdate()
    {
        if (PendingFractures.Count > 0) ProcessPendingFractures();
    }

    private static void ProcessPendingFractures()
    {
        if (!NetworkServer.active || !Run.instance)
        {
            PendingFractures.Clear();
            return;
        }

        for (int i = PendingFractures.Count - 1; i >= 0; i--)
        {
            InventoryReplacementCandidate inventoryReplacementCandidate = PendingFractures[i];
            if (inventoryReplacementCandidate.time.hasPassed)
            {
                if (!StepInventoryFracture(inventoryReplacementCandidate.inventory, inventoryReplacementCandidate.originalItem)) PendingFractures.RemoveAt(i);
                else
                {
                    PendingFractures[i] = inventoryReplacementCandidate;
                }
            }
        }
    }

    private static bool StepInventoryFracture(Inventory inventory, ItemIndex originalItemIndex)
    {
        if (itemIndex == ItemIndex.None) return false;

        var count = inventory.GetItemCount(originalItemIndex) - 1;
        if (count > 0)
        {
            inventory.RemoveItem(originalItemIndex, count);
            inventory.GiveItem(itemIndex, count);

            var characterMaster = inventory.GetComponent<CharacterMaster>();
            if (characterMaster) CharacterMasterNotificationQueue.SendTransformNotification(characterMaster, originalItemIndex, itemIndex, CharacterMasterNotificationQueue.TransformationType.LunarSun);
        }

        return true;
    }

    private void OnInventoryChangedGlobal(Inventory inventory)
    {
        if (!NetworkServer.active) return;
        foreach (ItemIndex itemIndex in inventory.itemAcquisitionOrder)
        {
            if (ItemCatalog.GetItemDef(itemIndex).tier != ItemTier.Lunar) continue;
            int itemCount = inventory.GetItemCount(itemIndex);
            if (itemCount > 1)
            {
                else inventory.RemoveItem(itemIndex, itemCount);
            }
        }
    }

    private static void TryQueueReplacement(Inventory inventory, ItemIndex originalItemIndex)
    {
        PendingFractures.Add(new InventoryReplacementCandidate
        {
            inventory = inventory,
            originalItem = originalItemIndex,
        });
    }

    private struct InventoryReplacementCandidate
    {
        public Inventory inventory;
        public ItemIndex originalItem;
        public Run.FixedTimeStamp time;
    }

    private static readonly List<InventoryReplacementCandidate> PendingFractures = [];
}

/*
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
*/