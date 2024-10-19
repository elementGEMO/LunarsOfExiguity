using RoR2;
using System.Collections.Generic;
using UnityEngine;

using static LunarsOfExiguity.ConsumeHooks;

namespace LunarsOfExiguity;
public class PurifiedTier : ItemTierBase
{
    protected override string Name => "Purified";

    public static ItemTierDef PurifiedItemTierDef;

    protected override ColorCatalog.ColorIndex Color => Colors.TempPureLight;
    protected override ColorCatalog.ColorIndex DarkColor => Colors.TempPureDark;

    protected override Texture IconBackgroundTexture => LoEPlugin.Bundle.LoadAsset<Sprite>("PureBackgroundIcon").texture;

    protected override bool CanBeScrapped => false;

    public static List<ItemIndex> ItemTierPool = [];
    public static List<ItemIndex> IgnoreBlemished = [];

    public static List<PurifiedFractureInfo> ItemCounterpartPool = [];

    protected override void Initialize()
    {
        PurifiedItemTierDef = Value;

        ItemCatalog.availability.onAvailable += SetUpPearls;
    }
    private static void SetUpPearls()
    {
        ItemDef pearlDef = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Pearl"));
        pearlDef.pickupIconSprite = LoEPlugin.Bundle.LoadAsset<Sprite>("PearlIcon");
        pearlDef._itemTierDef = PurifiedItemTierDef;
        ItemTierPool.Add(pearlDef.itemIndex);
        IgnoreBlemished.Add(pearlDef.itemIndex);

        ItemDef shinyPearlDef = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ShinyPearl"));
        shinyPearlDef.pickupIconSprite = LoEPlugin.Bundle.LoadAsset<Sprite>("ShinyPearlIcon");
        shinyPearlDef._itemTierDef = PurifiedItemTierDef;
        ItemTierPool.Add(shinyPearlDef.itemIndex);
        IgnoreBlemished.Add(shinyPearlDef.itemIndex);
    }
}