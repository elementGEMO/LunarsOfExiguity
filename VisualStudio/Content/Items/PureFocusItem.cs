using BepInEx.Configuration;
using RoR2;
using UnityEngine;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class PureFocusItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;
    public static ConfigEntry<int> Max_Damage_Percent;
    public static ConfigEntry<int> Percent_Loss_Hit;

    protected override string Name => "PureFocus";
    public static ItemDef ItemDef;

    protected override CombinedItemTier Tier => PurifiedTier.PurifiedItemTierDef;
    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("PureFocusIcon");
    protected override GameObject PickupModelPrefab => LoEPlugin.Bundle.LoadAsset<GameObject>("PureFocusModel");

    protected override string DisplayName => "Honor of Focus";
    protected override string CursedNameOverride => DisplayName;

    protected override string PickupText => SimplePickup + "Fractures Focused Convergence".Style("#D2B088") + ".";
    public static string SimplePickup = "All enemies after the Teleporter event take damage. ";
    protected override string Description => string.Format(SimpleDesc + "Fractures Focused Convergence".Style("#D2B088") + ".", Max_Damage_Percent.Value, Percent_Loss_Hit.Value);
    public static string SimpleDesc = "After the " + "Teleporter event".Style(FontColor.cIsUtility) + ", " + "enemies ".Style(FontColor.cIsHealth) + "lose " + "{0}% health".Style(FontColor.cIsHealth) + ", reduced by " + "{1}% ".Style(FontColor.cIsHealth) + "each time you were hit during it. ";

    /*
    protected override string Lore => "The tranquil murmur of the waters echo as a reminder. Their kin remains.\r\n\r\n" +
        "This shell, passed from one to another. Carrying the memories of lost remnants, and new experiences - their hopes, their struggles, their legacy.\r\n\r\n" +
        "Would you allow this cycle to flow, or will you claim it as your own?";
    */

    protected override bool IsEnabled()
    {
        Max_Damage_Percent = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Health Loss", 50,
            new ConfigDescription(
                "[ 50 = 50% Health Loss | on Enemies at Teleporter Complete ]",
                new AcceptableValueRange<int>(0, 100)
            )
        );
        Percent_Loss_Hit = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Percent Loss", 2,
            new ConfigDescription(
                "[ 2 = 2% Percent Loss | on Health Damage per Hit Recieved ]",
                new AcceptableValueRange<int>(0, 100)
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
    protected override void LogDisplay()
    {
        ModelPanelParameters modelParam = PickupModelPrefab.AddComponent<ModelPanelParameters>();
        var foundMesh = PickupModelPrefab.transform.GetChild(0);

        if (!foundMesh) return;

        modelParam.focusPointTransform = foundMesh;
        modelParam.cameraPositionTransform = foundMesh;
        modelParam.minDistance = 0.05f;
        modelParam.maxDistance = 0.25f;
        modelParam.modelRotation = new Quaternion(-0.0896857604f, 0.448245376f, -0.0226525813f, 0.889111578f);

        PickupModelPrefab.AddComponent<FloatingPointFix>();
    }

    private void PairFractured()
    {
        PurifiedTier.ItemCounterpartPool.Add(new PurifiedTier.PurifiedFractureInfo
        {
            purifiedItem = ItemDef.itemIndex,
            originalItem = ItemCatalog.FindItemIndex(FocusedConvergenceRework.StaticInternal)
        });
    }
}