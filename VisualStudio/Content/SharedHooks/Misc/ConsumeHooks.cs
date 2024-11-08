using RoR2;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace LunarsOfExiguity;
public class ConsumeHooks
{
    private List<ItemIndex> ItemBlacklist;
    public ConsumeHooks()
    {
        Inventory.onInventoryChangedGlobal += OnInventoryChangedGlobal;
        RoR2Application.onFixedUpdate += OnFixedUpdate;

        ItemCatalog.availability.onAvailable += AddBlacklist;
    }

    private void AddBlacklist()
    {
        ItemBlacklist.Add(DLC1Content.Items.LunarSun.itemIndex);
    }

    private void OnFixedUpdate()
    {
        if (PendingReplacement.Count > 0) ProcessPendingConsumed();
    }
    private static void ProcessPendingConsumed()
    {
        if (!NetworkServer.active || !Run.instance)
        {
            PendingReplacement.Clear();
            return;
        }

        for (int i = PendingReplacement.Count - 1; i >= 0; --i)
        {
            InventoryReplacementCandidate info = PendingReplacement[i];
            if (info.time.hasPassed)
            {
                if (StepInventoryConsumed(info)) PendingReplacement.RemoveAt(i);
                else PendingReplacement[i] = info;
            }
        }
    }

    private static bool StepInventoryConsumed(InventoryReplacementCandidate info)
    {
        ItemIndex itemIndex = info.transformItem.itemIndex;
        if (itemIndex == ItemIndex.None) return false;

        bool hasItem = info.inventory.GetItemCount(info.originalItem) > info.safeLock;

        if (hasItem) {
            info.inventory.RemoveItem(info.originalItem, info.itemCount);
            if (info.giveItem) info.inventory.GiveItem(itemIndex, info.itemCount);

            var characterMaster = info.inventory.GetComponent<CharacterMaster>();
            if (characterMaster) CharacterMasterNotificationQueue.SendTransformNotification(characterMaster, info.originalItem, itemIndex, CharacterMasterNotificationQueue.TransformationType.LunarSun);
        }
        
        return true;
    }

    private void OnInventoryChangedGlobal(Inventory inventory)
    {
        if (!NetworkServer.active) return;

        foreach (ItemIndex itemIndex in inventory.itemAcquisitionOrder)
        {
            bool isLunarTier = ItemCatalog.GetItemDef(itemIndex).tier == ItemTier.Lunar;
            bool isPureTier = ItemCatalog.GetItemDef(itemIndex).tier == PurifiedTier.PurifiedItemTierDef.tier;
            int itemCount = inventory.GetItemCount(itemIndex);



            if (itemIndex != DLC1Content.Items.LunarSun.itemIndex)
            {
                if (HasPureCounterpart(inventory, itemIndex)) TryReplacement(ConsumeType.Fractured, inventory, itemIndex, itemCount);
                else if (isLunarTier && itemCount > 1) TryReplacement(ConsumeType.Fractured, inventory, itemIndex, itemCount - 1, 1);
                else if (isPureTier && !IgnoresBlemished(itemIndex) && itemCount > 1) TryReplacement(ConsumeType.Blemished, inventory, itemIndex, itemCount - 1, 1);
            }
        }
    }

    private static bool HasPureCounterpart(Inventory inventory, ItemIndex itemIndex)
    {
        foreach (PurifiedTier.PurifiedFractureInfo pair in PurifiedTier.ItemCounterpartPool)
        {
            bool sameIndex = itemIndex == pair.originalItem;
            if (!sameIndex) continue;

            bool hasPure = inventory.GetItemCount(pair.purifiedItem) > 0;
            if (hasPure) return true;
        }

        return false;
    }
    private static bool IgnoresBlemished(ItemIndex itemIndex) => PurifiedTier.IgnoreBlemished.Contains(itemIndex);

    private static void TryReplacement(ConsumeType target, Inventory inventory, ItemIndex originalItemIndex, int itemCount, int safety = 0)
    {
        if (target == ConsumeType.Fractured)
        {
            PendingReplacement.Add(new InventoryReplacementCandidate
            {
                safeLock = safety,
                inventory = inventory,
                itemCount = itemCount,
                originalItem = originalItemIndex,
                transformItem = FracturedItem.ItemDef,
                giveItem = FracturedItem.Gain_Fracture.Value
            });
        }
        else
        {
            PendingReplacement.Add(new InventoryReplacementCandidate
            {
                safeLock = safety,
                inventory = inventory,
                itemCount = itemCount,
                originalItem = originalItemIndex,
                transformItem = BlemishedItem.ItemDef,
                giveItem = BlemishedItem.Gain_Blemished.Value
            });
        }
    }

    private struct InventoryReplacementCandidate
    {
        public int safeLock;
        public int itemCount;
        public bool giveItem;
        public Inventory inventory;
        public ItemIndex originalItem;
        public ItemDef transformItem;

        public Run.FixedTimeStamp time;
    }

    private static readonly List<InventoryReplacementCandidate> PendingReplacement = [];
    private enum ConsumeType
    {
        Fractured,
        Blemished
    }
}