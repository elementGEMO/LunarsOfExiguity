using BepInEx.Configuration;
using RoR2;
using UnityEngine;

namespace LunarsOfExiguity;
public class ShrineCleanseRework
{
    public static ConfigEntry<int> Irradiant_Chance;
    public static string StaticInternal = "Cleansing Pool";
    public ShrineCleanseRework() => On.RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.PayCost += ReplaceWithPure;

    private void ReplaceWithPure(On.RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.orig_PayCost orig, CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
    {
        orig(costTypeDef, context);

        ShopTerminalBehavior selfShop = context.purchasedObject.GetComponent<ShopTerminalBehavior>();
        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MonstersOnShrineUse"), new EffectData
        {
            origin = context.purchasedObject.transform.position,
            rotation = context.purchasedObject.transform.rotation,
        }, true);

        if (selfShop)
        {
            ItemIndex pureResult = ItemCatalog.FindItemIndex("Pearl"); ;
            if (Util.CheckRoll(Irradiant_Chance.Value)) pureResult = ItemCatalog.FindItemIndex("ShinyPearl");

            if (context.results.itemsTaken.Count > 0)
            {
                ItemIndex foundItem = context.results.itemsTaken[0];
                foreach (PurifiedTier.PurifiedFractureInfo pair in PurifiedTier.ItemCounterpartPool)
                {
                    bool sameIndex = foundItem == pair.originalItem;
                    if (sameIndex) { pureResult = pair.purifiedItem; break; }
                }
            }

            selfShop.SetPickupIndex(new PickupIndex(pureResult));
        }
    }
}
