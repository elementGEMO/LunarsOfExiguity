//"PureLightFluxModel"

using BepInEx.Configuration;
using RoR2;
using UnityEngine;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class PureStoneFluxItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;
    public static ConfigEntry<float> Size_Modifier;
    public static ConfigEntry<int> Armor_Gain;

    protected override string Name => "PureStoneFlux";
    public static ItemDef ItemDef;

    protected override CombinedItemTier Tier => PurifiedTier.PurifiedItemTierDef;
    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("PureStoneFluxIcon");
    protected override GameObject PickupModelPrefab => LoEPlugin.Bundle.LoadAsset<GameObject>("PureStoneFluxModel");

    protected override string DisplayName => "Honor of Acromegaly";
    protected override string CursedNameOverride => DisplayName;

    protected override string PickupText => SimplePickup + "Fractures Stone Flux Pauldron".Style("#D2B088") + ".";
    public static string SimplePickup = "Gain armor and grow while in danger. ";
    protected override string Description => string.Format(SimpleDesc + "Fractures Stone Flux Pauldron".Style("#D2B088") + ".", Armor_Gain.Value, RoundVal(Size_Modifier.Value));
    public static string SimpleDesc = "While in danger, increase " + "armor ".Style(FontColor.cIsHealing) + "by " + "{0} ".Style(FontColor.cIsHealing) + "and increase your " + "size ".Style(FontColor.cIsUtility) + "by " + "{1}%".Style(FontColor.cIsUtility) + ". ";

    /*
    protected override string Lore => "The tranquil murmur of the waters echo as a reminder. Their kin remains.\r\n\r\n" +
        "This shell, passed from one to another. Carrying the memories of lost remnants, and new experiences - their hopes, their struggles, their legacy.\r\n\r\n" +
        "Would you allow this cycle to flow, or will you claim it as your own?";
    */

    protected override bool IsEnabled()
    {
        Size_Modifier = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Size Modifier", 25f,
            "[ 25 = 25% Size Increase | in Danger ]"
        );
        Armor_Gain = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Armor Gain", 100,
            "[ 100 = +100 Armor | in Danger ]"
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
        modelParam.modelRotation = new Quaternion(0.0115291597f, -0.587752283f, 0.0455321521f, -0.807676435f);

        PickupModelPrefab.AddComponent<FloatingPointFix>();
    }

    private void PairFractured()
    {
        PurifiedTier.ItemCounterpartPool.Add(new PurifiedTier.PurifiedFractureInfo
        {
            purifiedItem = ItemDef.itemIndex,
            originalItem = ItemCatalog.FindItemIndex(StoneFluxRework.StaticInternal)
        });
    }
}