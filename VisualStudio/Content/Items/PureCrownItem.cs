using BepInEx.Configuration;
using RoR2;
using R2API;
using UnityEngine;

using static LoEUtils;
using static LoEColors;
using static LoERenderHelper;

namespace LunarsOfExiguity;
public class PureCrownItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;
    public static ConfigEntry<float> Chance_Free;

    protected override string Name => "PureCrown";
    public static ItemDef ItemDef;

    protected override CombinedItemTier Tier => PurifiedTier.PurifiedItemTierDef;
    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("PureCrownIcon");
    protected override GameObject PickupModelPrefab => LoEPlugin.Bundle.LoadAsset<GameObject>("PureCrownModel");

    protected override string DisplayName => "Honor of Greed";
    protected override string CursedNameOverride => "Honor of Debt";
    protected override string PickupText => SimplePickup + "Fractures Brittle Crown".Style("#D2B088") + ".";
    public static string SimplePickup = "All cash interactables have a chance to be free. ";

    protected override string Description => string.Format(SimpleDesc + "Fractures Brittle Crown".Style("#D2B088") + ".", Chance_Free.Value);
    public static string SimpleDesc = "All " + "gold purchase interactables ".Style(FontColor.cIsUtility) + "have a " + "{0}% ".Style(FontColor.cIsUtility) + "chance to be " + "free".Style(FontColor.cIsUtility) + ". ";

    /*
    protected override string Lore => "The tranquil murmur of the waters echo as a reminder. Their kin remains.\r\n\r\n" +
        "This shell, passed from one to another. Carrying the memories of lost remnants, and new experiences - their hopes, their struggles, their legacy.\r\n\r\n" +
        "Would you allow this cycle to flow, or will you claim it as your own?";
    */

    protected override bool IsEnabled()
    {
        Chance_Free = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Chance for Free Chest", 15f,
            new ConfigDescription(
                "[ 15 = 15% Chance | Free for each Gold Interactable ]",
                new AcceptableValueRange<float>(0, 100)
            )
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
        baseDisplay.Add("CommandoBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HeadCenter",
            localPos = new Vector3(0f, 0.115f, -0.025f),
            localAngles = new Vector3(345f, 360f, 0f),
            localScale = Vector3.one * 12f
        });

        baseDisplay.Add("HuntressBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HeadCenter",
            localPos = new Vector3(0f, 0.1f, -0.06f),
            localAngles = new Vector3(337.5f, 1f, 0f),
            localScale = Vector3.one * 10f
        });

        baseDisplay.Add("Bandit2Body", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Hat",
            localPos = new Vector3(0f, 0.120f, -0.042f),
            localAngles = new Vector3(337.5f, 0f, 0f),
            localScale = Vector3.one * 10f
        });

        baseDisplay.Add("ToolbotBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 2.57f, 1.73f),
            localAngles = new Vector3(75.95f, 0f, 0f),
            localScale = Vector3.one * 75f
        });

        baseDisplay.Add("EngiBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HeadCenter",
            localPos = new Vector3(0f, 0.18f, -0.01f),
            localAngles = new Vector3(0f, 0f, 0f),
            localScale = Vector3.one * 11f
        });
        baseDisplay.Add("EngiTurretBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 1.105f, -1.425f),
            localAngles = new Vector3(332.5f, 0f, 0f),
            localScale = Vector3.one * 25f
        });
        baseDisplay.Add("EngiWalkerTurretBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 1.725f, -0.58f),
            localAngles = new Vector3(0f, 0f, 0f),
            localScale = Vector3.one * 25f
        });

        baseDisplay.Add("MageBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HeadCenter",
            localPos = new Vector3(0f, 0.09f, -0.075f),
            localAngles = new Vector3(4.9f, 0f, 0f),
            localScale = Vector3.one * 6f
        });

        baseDisplay.Add("MercBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HeadCenter",
            localPos = new Vector3(0f, 0.11f, 0f),
            localAngles = new Vector3(3.95f, 0f, 0f),
            localScale = Vector3.one * 9f
        });

        baseDisplay.Add("TreebotBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HeadCenter",
            localPos = new Vector3(0.05f, -0.05f, 0f),
            localAngles = new Vector3(90f, 90f, 0f),
            localScale = new Vector3(10f, 10f, 3.5f)
        });

        baseDisplay.Add("LoaderBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HeadCenter",
            localPos = new Vector3(0f, 0.140f, 0.0275f),
            localAngles = new Vector3(8.9f, 0f, 0f),
            localScale = Vector3.one * 9f
        });

        baseDisplay.Add("CrocoBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HeadCenter",
            localPos = new Vector3(0f, 0.195f, 1.150f),
            localAngles = new Vector3(65f, 180f, 180f),
            localScale = Vector3.one * 125f
        });

        baseDisplay.Add("CaptainBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.275f, 0.075f),
            localAngles = new Vector3(13.875f, 0f, 0f),
            localScale = Vector3.one * 10.5f
        });

        baseDisplay.Add("HereticBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(-0.255f, -0.072f, 0f),
            localAngles = new Vector3(302f, 270f, 180f),
            localScale = Vector3.one * 15f
        });

        // Survivors of the Void
        baseDisplay.Add("RailgunnerBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.210f, -0.020f),
            localAngles = new Vector3(356.520f, 0f, 0f),
            localScale = Vector3.one * 9f
        });

        baseDisplay.Add("VoidSurvivorBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.135f, -0.085f),
            localAngles = new Vector3(317.950f, 0f, 0f),
            localScale = Vector3.one * 11.5f
        });

        // Seekers of the Storm
        baseDisplay.Add("SeekerBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.215f, 0.018f),
            localAngles = new Vector3(3.675f, 0f, 0f),
            localScale = Vector3.one * 9f
        });

        baseDisplay.Add("FalseSonBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.425f, -0.003f),
            localAngles = new Vector3(357.625f, 0f, 0f),
            localScale = new Vector3(16f, 16f, 16.5f)
        });

        baseDisplay.Add("ChefBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(-0.9f, -0.390f, -0.047f),
            localAngles = new Vector3(314.215f, 267.225f, 173.125f),
            localScale = Vector3.one * 15f
        });

        // Starstorm 2
        baseDisplay.Add("NemCommandoBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 1.250f, -0.007f),
            localAngles = new Vector3(7.15f, 1.96f, 2.03f),
            localScale = Vector3.one * 45f
        });

        baseDisplay.Add("NemMercBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.25f, 0f),
            localAngles = new Vector3(0f, 0f, 0f),
            localScale = Vector3.one * 9f
        });

        baseDisplay.Add("Executioner2Body", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HeadCenter",
            localPos = new Vector3(0f, 0.167f, -0.325f),
            localAngles = new Vector3(345f, 0f, 0f),
            localScale = Vector3.one * 10f
        });

        baseDisplay.Add("ChirrBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0f, 0.575f),
            localAngles = new Vector3(90f, 0f, 0f),
            localScale = Vector3.one * 30f
        });

        // PaladinMod
        baseDisplay.Add("RobPaladinBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HeadCenter",
            localPos = new Vector3(0f, 0.45f, 0.09f),
            localAngles = new Vector3(13.16f, 0f, 0f),
            localScale = Vector3.one * 14f
        });

        // RobomandoMod
        baseDisplay.Add("RobomandoBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.3f, 0f),
            localAngles = new Vector3(0f, 90f, 0f),
            localScale = Vector3.one * 13f
        });

        // DriverMod
        baseDisplay.Add("RobDriverBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.262f, 0f),
            localAngles = new Vector3(0f, 0f, 0f),
            localScale = Vector3.one * 10f
        });

        // HANDOverclockedMod
        baseDisplay.Add("HANDOverclockedBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Hat",
            localPos = new Vector3(0f, 0.657f, 0f),
            localAngles = new Vector3(0f, 330f, 0f),
            localScale = Vector3.one * 50f
        });

        // EnforcerMod
        baseDisplay.Add("EnforcerBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.275f, 0f),
            localAngles = new Vector3(0f, 90f, 0f),
            localScale = Vector3.one * 12.5f
        });
        baseDisplay.Add("NemesisEnforcerBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.01f, 0f),
            localAngles = new Vector3(0f, 90f, 0f),
            localScale = Vector3.one * 0.35f
        });

        // MinerUnearthedMod
        baseDisplay.Add("MinerBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.003f, 0f),
            localAngles = new Vector3(0f, 0f, 0f),
            localScale = Vector3.one * 0.105f
        });

        // RavagerMod
        baseDisplay.Add("RobRavagerBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.37f, 0f),
            localAngles = new Vector3(0f, 0f, 0f),
            localScale = Vector3.one * 10f
        });

        // ArsonistMod
        baseDisplay.Add("ArsonistBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0f, 0.2f, 0.025f),
            localAngles = new Vector3(0f, 0f, 0f),
            localScale = Vector3.one * 9f
        });

        // RocketMod
        baseDisplay.Add("RocketSurvivorBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "head",
            localPos = new Vector3(-0.015f, 0.225f, 0f),
            localAngles = new Vector3(0f, 90f, 0f),
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

        PickupModelPrefab.AddComponent<FloatingPointFix>();
    }
    private void PairFractured()
    {
        PurifiedTier.ItemCounterpartPool.Add(new PurifiedTier.PurifiedFractureInfo
        {
            purifiedItem = ItemDef.itemIndex,
            originalItem = ItemCatalog.FindItemIndex(BrittleCrownRework.StaticInternal)
        });
    }
}