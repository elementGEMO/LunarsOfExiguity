using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace LunarsOfExiguity;
public class PurifiedTier : ItemTierBase
{
    protected override string Name => "Purified";

    public static ItemTierDef PurifiedItemTierDef;

    protected override ColorCatalog.ColorIndex Color => ColorsAPI.RegisterColor(new Color32(210, 176, 136, 255)); //Colors.TempPureLight;
    protected override ColorCatalog.ColorIndex DarkColor => ColorsAPI.RegisterColor(new Color32(173, 146, 108, 255)); //Colors.TempPureDark;

    protected override Texture IconBackgroundTexture => LoEPlugin.Bundle.LoadAsset<Sprite>("PureBackgroundIcon").texture;

    protected override bool CanBeScrapped => false;

    public static List<ItemIndex> ItemTierPool = [];
    public static List<ItemIndex> IgnoreBlemished = [];

    public static List<PurifiedFractureInfo> ItemCounterpartPool = [];

    public struct PurifiedFractureInfo
    {
        public ItemIndex originalItem;
        public ItemIndex purifiedItem;
    }

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