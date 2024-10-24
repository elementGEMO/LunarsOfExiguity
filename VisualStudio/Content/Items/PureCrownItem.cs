using BepInEx.Configuration;
using RoR2;
using UnityEngine;

using static LoEUtils;
using static LoEColors;
using R2API;

namespace LunarsOfExiguity;
public class PureCrownItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;

    protected override string Name => "PureCrown";
    public static ItemDef ItemDef;

    protected override CombinedItemTier Tier => PurifiedTier.PurifiedItemTierDef;
    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("PureCrownIcon");
    protected override GameObject PickupModelPrefab => LoEPlugin.Bundle.LoadAsset<GameObject>("PureCrownModel");

    protected override string DisplayName => "Honor of Greed";
    protected override string CursedNameOverride => "Honor of Debt";

    /*
    protected override string PickupText => SimplePickup + "Fractures Gesture of the Drowned".Style("#D2B088") + ".";
    public static string SimplePickup = "Chance on Equipment activation to not use charge. ";

    protected override string Description => string.Format(SimpleDesc + "Fractures Gesture of the Drowned".Style("#D2B088") + ".", Percent_Chance.Value);
    public static string SimpleDesc = "Activating your Equipment has a " + "{0}% ".Style(FontColor.cIsUtility) + "chance to " + "not use charge".Style(FontColor.cIsUtility) + ". ";

    protected override string Lore => "The tranquil murmur of the waters echo as a reminder. Their kin remains.\r\n\r\n" +
        "This shell, passed from one to another. Carrying the memories of lost remnants, and new experiences - their hopes, their struggles, their legacy.\r\n\r\n" +
        "Would you allow this cycle to flow, or will you claim it as your own?";
    */

    protected override bool IsEnabled()
    {
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
        ItemDisplayRuleDict baseDisplay = new();

        baseDisplay.Add("CommandoBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "HeadCenter",
            localPos = new Vector3(0f, 0.115f, -0.025f),
            localAngles = new Vector3(345f, 360f, 0f),
            localScale = new Vector3(12f, 12f, 12f)
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