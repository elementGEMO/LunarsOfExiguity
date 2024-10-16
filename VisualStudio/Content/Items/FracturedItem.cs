﻿using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace LunarsOfExiguity.Content.Items;

public class FracturedItem : ItemBase
{
    protected override string Name => "Fractured";

    protected override GameObject PickupModelPrefab { get; }
    protected override Sprite PickupIconSprite { get; }
    
    protected override ItemTag[] Tags => [ItemTag.CannotCopy, ItemTag.CannotSteal, ItemTag.CannotDuplicate];

    protected override string DisplayName => "Fractured";
    protected override string PickupText => "Your <style=cIsLunar>lunar</style items have shattered into pieces.";
  
    protected override void Initialize()
    {
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
                    inventoryReplacementCandidate.time = Run.FixedTimeStamp.now + LoEConfig.FractureItemDelay.Value;
                    PendingFractures[i] = inventoryReplacementCandidate;
                }
            }
        }
    }
    
    private static bool StepInventoryFracture(Inventory inventory, ItemIndex originalItemIndex)
    {
        ItemIndex itemIndex = ItemCatalog.FindItemIndex("Fractured");
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
                if (LoEConfig.GainItemOnFracture.Value) TryQueueReplacement(inventory, itemIndex);
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
            time = Run.FixedTimeStamp.now + LoEConfig.FractureItemDelay.Value
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