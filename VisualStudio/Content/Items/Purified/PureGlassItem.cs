//"PureLightFluxModel"

using BepInEx.Configuration;
using RoR2;
using UnityEngine;

using static LoEUtils;
using static LoEColors;

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
    public static string SimplePickup = "Gain more damage the lower health you are. ";
    protected override string Description => string.Format(SimpleDesc + "Fractures Shaped Glass".Style("#D2B088") + ".");
    public static string SimpleDesc = ". ";

    /*
    protected override string Lore => "The tranquil murmur of the waters echo as a reminder. Their kin remains.\r\n\r\n" +
        "This shell, passed from one to another. Carrying the memories of lost remnants, and new experiences - their hopes, their struggles, their legacy.\r\n\r\n" +
        "Would you allow this cycle to flow, or will you claim it as your own?";
    */

    protected override bool IsEnabled()
    {
        Damage_Modifier = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Damage Multiplier", 100f,
            "[ 100 = 100% Base Damage | Total at Very Low Health ]"
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
            originalItem = ItemCatalog.FindItemIndex(GlassRework.StaticInternal)
        });
    }
}