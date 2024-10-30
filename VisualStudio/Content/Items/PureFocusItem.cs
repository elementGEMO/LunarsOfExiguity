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

    protected override string Lore => "They showed their thanks to their protectors, decorating their homes with carvings and statues constructed in their image.\n\n" +
        "And for that, they were blessed.\n\n" +
        "They showed their thanks to their savior, laboring to create elaborate temples, cast in shadows by the image of their hero which broke up the horizon.\n\n" +
        "And for that, they were blessed.\n\n" +
        "They showed their thanks to the gates that saved them. A misunderstanding of their purpose, a mockery of a divine instrument by a creator long forgotten.\n\n" +
        "And for that, they were blessed.";

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