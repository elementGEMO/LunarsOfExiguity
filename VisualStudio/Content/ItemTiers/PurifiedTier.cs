using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LunarsOfExiguity;
public class PurifiedTier : ItemTierBase
{
    protected override string Name => "Purified";

    public static ItemTierDef PurifiedItemTierDef;

    protected override Texture IconBackgroundTexture => LoEPlugin.Bundle.LoadAsset<Sprite>("PureBackgroundIcon").texture;

    protected override ColorCatalog.ColorIndex Color => Colors.TempPureLight;
    protected override ColorCatalog.ColorIndex DarkColor => Colors.TempPureDark;

    public static List<ItemDef> ItemTierPool => [];

    protected override void Initialize()
    {
        PurifiedItemTierDef = Value;

        ItemDef pearlDef = Addressables.LoadAsset<ItemDef>("RoR2/Base/Pearl/Pearl.asset").WaitForCompletion();
        pearlDef.pickupIconSprite = LoEPlugin.Bundle.LoadAsset<Sprite>("PearlIcon");
        pearlDef._itemTierDef = PurifiedItemTierDef;
        ItemTierPool.Add(pearlDef);

        ItemDef shinyPearlDef = Addressables.LoadAsset<ItemDef>("RoR2/Base/ShinyPearl/ShinyPearl.asset").WaitForCompletion();
        shinyPearlDef.pickupIconSprite = LoEPlugin.Bundle.LoadAsset<Sprite>("ShinyPearlIcon");
        shinyPearlDef._itemTierDef = PurifiedItemTierDef;
        ItemTierPool.Add(shinyPearlDef);
    }
}