using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LunarsOfExiguity;
public class PurifiedTier : ItemTierBase
{
    protected override string Name => "Purified";

    public static ItemTierDef PurifiedItemTierDef;

    protected override Texture IconBackgroundTexture => LoEPlugin.Bundle.LoadAsset<Sprite>("PureBackgroundIcon").texture;

    protected override ColorCatalog.ColorIndex Color => ColorCatalog.ColorIndex.BossItem;
    protected override ColorCatalog.ColorIndex DarkColor => ColorCatalog.ColorIndex.BossItemDark;

    protected override void Initialize()
    {
        PurifiedItemTierDef = Value;

        ItemDef pearlDef = Addressables.LoadAsset<ItemDef>("RoR2/Base/Pearl/Pearl.asset").WaitForCompletion();
        pearlDef.pickupIconSprite = LoEPlugin.Bundle.LoadAsset<Sprite>("PearlIcon");
        pearlDef._itemTierDef = PurifiedItemTierDef;

        ItemDef shinyPearlDef = Addressables.LoadAsset<ItemDef>("RoR2/Base/ShinyPearl/ShinyPearl.asset").WaitForCompletion();
        shinyPearlDef.pickupIconSprite = LoEPlugin.Bundle.LoadAsset<Sprite>("ShinyPearlIcon");
        shinyPearlDef._itemTierDef = PurifiedItemTierDef;
    }
}