using R2API;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;

using static LoEUtils;
using static LoEColors;
using static LoERenderHelper;

namespace LunarsOfExiguity;
public class PureGlassItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;
    public static ConfigEntry<float> Damage_Modifier;

    protected override string Name => "PureLunarDagger";
    public static ItemDef ItemDef;

    protected override CombinedItemTier Tier => PurifiedTier.PurifiedItemTierDef;
    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("PureGlassIcon");
    protected override GameObject PickupModelPrefab => LoEPlugin.Bundle.LoadAsset<GameObject>("PureGlassModel");

    protected override string DisplayName => "Honor of Glass";
    protected override string CursedNameOverride => DisplayName;

    protected override string PickupText => SimplePickup + "Fractures Shaped Glass".Style("#D2B088") + ".";
    public static string SimplePickup = "Gain more damage the less health you have. ";
    protected override string Description => string.Format(SimpleDesc + "Fractures Shaped Glass".Style("#D2B088") + ".", RoundVal(Damage_Modifier.Value));
    public static string SimpleDesc = "Increase base damage by " + "{0}% ".Style(FontColor.cIsDamage) + "as your health decreases. ";

    protected override string Lore => "I am falling once more. The weight of my blade is gone - broken, stolen, I can't tell. It scarcely matters now; death waits below. I feel lighter, strangely. Is it from the absence of my blade, or something deeper within me slipping away?\n\n" +
        "All I remember is falling to my knees, as I see a silhouette with many red eyes, watching. Perhaps it is time for another to take up my mantle as I fade.\n\n" +
        "My mind is fracturing, just for a moment.\n\n" +
        "...\n\n" +
        "I'm sorry, brother.";

    protected override bool IsEnabled()
    {
        Damage_Modifier = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Damage Multiplier", 200f,
            "[ 200 = 200% Base Damage | Total at Very Low Health ]"
        );
        Item_Enabled = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Enable Item", true,
            "[ True = Enabled | False = Disabled ]"
        );

        return Item_Enabled.Value;
    }

    protected override void Initialize()
    {
        ItemDef = Value;
        PurifiedTier.ItemTierPool.Add(ItemDef.itemIndex);
        ItemCatalog.availability.onAvailable += PairFractured;
    }
    protected override ItemDisplayRuleDict ItemDisplay()
    {

        PickupModelPrefab.AddComponent<ItemDisplay>().rendererInfos = ItemDisplaySetup(PickupModelPrefab);
        ItemDisplayRuleDict baseDisplay = new();

        // Risk of Rain 2
        baseDisplay.Add("MercBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HandL",
            localPos = new Vector3(-0.616f, 0.155f, -0.225f),
            localAngles = new Vector3(13.87f, 339.335f, 42.475f),
            localScale = Vector3.one * 5f
        });

        baseDisplay.Add("HereticBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Chest",
            localPos = new Vector3(-0.441f, 0.123f, -0.183f),
            localAngles = new Vector3(326.186f, 339.533f, 325.215f),
            localScale = Vector3.one * 10f
        });

        baseDisplay.Add("FalseSonBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HandR",
            localPos = new Vector3(-1.401f, 0.410f, -0.05f),
            localAngles = new Vector3(0f, 0f, 37.72f),
            localScale = Vector3.one * 10f
        });

        return baseDisplay;
    }
    protected override void LogDisplay()
    {
        ModelPanelParameters modelParam = PickupModelPrefab.AddComponent<ModelPanelParameters>();
        var foundMesh = PickupModelPrefab.transform.GetChild(0);

        if (!foundMesh) return;

        modelParam.focusPointTransform = foundMesh;
        modelParam.cameraPositionTransform = foundMesh;
        modelParam.minDistance = 0.05f;
        modelParam.maxDistance = 0.25f;
        modelParam.modelRotation = new Quaternion(0.0115291597f, -0.587752283f, 0.0455321521f, -0.807676435f);

        PickupModelPrefab.AddComponent<FloatingPointFix>();
    }

    private void PairFractured()
    {
        PurifiedTier.ItemCounterpartPool.Add(new PurifiedTier.PurifiedFractureInfo
        {
            purifiedItem = ItemDef.itemIndex,
            originalItem = ItemCatalog.FindItemIndex(GlassRework.StaticInternal)
        });
    }
}