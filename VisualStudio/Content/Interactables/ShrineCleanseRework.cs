using RoR2;

namespace LunarsOfExiguity;
public class ShrineCleanseRework
{
    public ShrineCleanseRework()
    {
        On.RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.PayCost += ReplaceWithPure;
    }

    private void ReplaceWithPure(On.RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.orig_PayCost orig, CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
    {
        orig(costTypeDef, context);

        ShopTerminalBehavior selfShop = context.purchasedObject.GetComponent<ShopTerminalBehavior>();
        ItemIndex foundItem = context.results.itemsTaken[0];

        if (selfShop)
        {
            ItemIndex pureResult = ItemCatalog.FindItemIndex("Pearl"); ;
            if (Util.CheckRoll(20)) pureResult = ItemCatalog.FindItemIndex("ShinyPearl");

            foreach (PurifiedTier.PurifiedFractureInfo pair in PurifiedTier.ItemCounterpartPool)
            {
                bool sameIndex = foundItem == pair.originalItem;
                if (sameIndex) { pureResult = pair.purifiedItem; break; }
            }

            selfShop.SetPickupIndex(new PickupIndex(pureResult));
        }
    }
}
