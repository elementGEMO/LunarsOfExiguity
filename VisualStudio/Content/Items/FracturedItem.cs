using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx.Configuration;
using System.Collections.Generic;

using static LoEColors;

namespace LunarsOfExiguity;
public class FracturedItem : ItemBase
{
    public static ConfigEntry<float> Fracture_Delay;
    public static ConfigEntry<bool> Gain_Fracture;

    protected override string Name => "Fractured";
    public static ItemDef ItemDef;

    protected override GameObject PickupModelPrefab { get; }
    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("IconLunarConsumed");

    protected override ItemTag[] Tags => [ItemTag.CannotCopy, ItemTag.CannotSteal, ItemTag.CannotDuplicate];

    protected override string DisplayName => "Fractured";
    protected override string PickupText => "Your " + "Lunar ".Style(FontColor.cIsLunar) + "items have shattered into pieces.";

    protected override bool IsEnabled()
    {
        Fracture_Delay = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item", "Fractured Delay", 0.5f,
            "[ 0.5 = 0.5s Fracture Notificaion Delay ]"
        );

        Gain_Fracture = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item", "Gain Fractured Item", true,
            "[ True = Gain 'Fractured' when a Lunar is Fractured | False = Nothing Happens ]"
        );

        return true;
    }

    protected override void Initialize()
    {
        ItemDef = Value;

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
        ItemIndex itemIndex = ItemDef.itemIndex;
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
                if (Gain_Fracture.Value) TryQueueReplacement(inventory, itemIndex);
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