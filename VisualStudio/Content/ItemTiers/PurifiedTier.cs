using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using static RoR2.ColorCatalog;

namespace LunarsOfExiguity;
public class PurifiedTier : ItemTierBase
{
    protected override string Name => "Purified";

    public static ItemTierDef PurifiedItemTierDef;

    protected override ColorIndex Color => ColorsAPI.RegisterColor(new Color32(210, 176, 136, 255)); //Colors.TempPureLight;
    protected override ColorIndex DarkColor => ColorsAPI.RegisterColor(new Color32(173, 146, 108, 255)); //Colors.TempPureDark;

    protected override Texture IconBackgroundTexture => LoEPlugin.Bundle.LoadAsset<Sprite>("PureBackgroundIcon").texture;
    protected override GameObject DropletDisplayPrefab => CreateDroplet();

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
    private GameObject CreateDroplet()
    {
        GameObject orbDrop = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/LunarOrb.prefab").WaitForCompletion().InstantiateClone("PurifiedOrb", true);
        Color setColor = GetColor(DarkColor);

        var trail = orbDrop.GetComponentInChildren<TrailRenderer>();
        if (trail)
        {
            trail.startColor = setColor;
            trail.set_startColor_Injected(ref setColor);
        }

        foreach (ParticleSystem particle in orbDrop.GetComponentsInChildren<ParticleSystem>())
        {
            var main = particle.main;
            var colorLifetime = particle.colorOverLifetime;

            main.startColor = new ParticleSystem.MinMaxGradient(setColor);
            colorLifetime.color = setColor;
        }

        return orbDrop;
    }
    private static void SetUpPearls()
    {
        ItemDef pearlDef = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("Pearl"));
        pearlDef.pickupIconSprite = LoEPlugin.Bundle.LoadAsset<Sprite>("PearlIcon");
        LanguageAPI.AddOverlay(pearlDef.nameToken, "Honor of Clarity");
        pearlDef._itemTierDef = PurifiedItemTierDef;
        ItemTierPool.Add(pearlDef.itemIndex);
        IgnoreBlemished.Add(pearlDef.itemIndex);

        ItemDef shinyPearlDef = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex("ShinyPearl"));
        shinyPearlDef.pickupIconSprite = LoEPlugin.Bundle.LoadAsset<Sprite>("ShinyPearlIcon");
        LanguageAPI.AddOverlay(shinyPearlDef.nameToken, "Honor of Radiance");
        shinyPearlDef._itemTierDef = PurifiedItemTierDef;
        ItemTierPool.Add(shinyPearlDef.itemIndex);
        IgnoreBlemished.Add(shinyPearlDef.itemIndex);
    }
}