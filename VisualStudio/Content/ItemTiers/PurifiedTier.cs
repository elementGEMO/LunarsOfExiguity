using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;


public class PurifiedTier : ItemTierBase
{
    protected override string Name => "Purified";

    public static ItemTierDef PurifiedItemTierDef;

    protected override Texture BackgroundTexture => LoEPlugin.Bundle.LoadAsset<Sprite>("PurifiedBGIcon").texture;

    protected override ColorCatalog.ColorIndex Color => ColorCatalog.ColorIndex.BossItem;
    protected override ColorCatalog.ColorIndex DarkColor => ColorCatalog.ColorIndex.BossItemDark;

    // Unsure why you would want this, but to ensure no change from source, I'll mark it false,

    protected override void Initialize()
    {
        PurifiedItemTierDef = Value;

        ItemDef pearlDef = Addressables.LoadAsset<ItemDef>("RoR2/Base/Pearl/Pearl.asset").WaitForCompletion();
        pearlDef._itemTierDef = PurifiedItemTierDef;
        //pearlDef.deprecatedTier = PurifiedItemTierDef.tier;

        //Log.Debug("Tier: " + PurifiedItemTierDef + ", Name: " + PurifiedItemTierDef.name + ", Tier: " + PurifiedItemTierDef.tier);

        /*
        ItemCatalog.availability.CallWhenAvailable(() =>
        {
        });
        */
    }
}