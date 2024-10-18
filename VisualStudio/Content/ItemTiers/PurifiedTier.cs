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

    protected override ColorCatalog.ColorIndex Color => Colors.TempPureLight;
    protected override ColorCatalog.ColorIndex DarkColor => Colors.TempPureDark;

    protected override Texture IconBackgroundTexture => LoEPlugin.Bundle.LoadAsset<Sprite>("PureBackgroundIcon").texture;

    protected override bool CanBeScrapped => false;

    public static List<ItemIndex> ItemTierPool => [];
    //public static List<ItemReworkBase> ItemCounterpartPool => [];
    public static List<ItemIndex> ConversionPure => [];
    public static List<ItemIndex> ConversionRework => [];

    protected override void Initialize()
    {
        PurifiedItemTierDef = Value;

        ItemDef pearlDef = Addressables.LoadAsset<ItemDef>("RoR2/Base/Pearl/Pearl.asset").WaitForCompletion();
        pearlDef.pickupIconSprite = LoEPlugin.Bundle.LoadAsset<Sprite>("PearlIcon");
        pearlDef._itemTierDef = PurifiedItemTierDef;
        ItemTierPool.Add(pearlDef.itemIndex);

        ItemDef shinyPearlDef = Addressables.LoadAsset<ItemDef>("RoR2/Base/ShinyPearl/ShinyPearl.asset").WaitForCompletion();
        shinyPearlDef.pickupIconSprite = LoEPlugin.Bundle.LoadAsset<Sprite>("ShinyPearlIcon");
        shinyPearlDef._itemTierDef = PurifiedItemTierDef;
        ItemTierPool.Add(shinyPearlDef.itemIndex);
    }
}