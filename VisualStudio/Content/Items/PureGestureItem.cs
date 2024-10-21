﻿using BepInEx.Configuration;
using RoR2;
using UnityEngine;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class PureGestureItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;
    public static ConfigEntry<int> Percent_Chance;

    protected override string Name => "PureGesture";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => PurifiedTier.PurifiedItemTierDef;
    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("PureGestureIcon");
    protected override GameObject PickupModelPrefab => LoEPlugin.Bundle.LoadAsset<GameObject>("PureGestureModel");

    protected override string DisplayName => "Honor of the Drowned";
    protected override string CursedNameOverride => DisplayName;

    protected override string PickupText => SimplePickup + "Fractures Gesture of the Drowned".Style("#D2B088") + ".";
    public static string SimplePickup = "Chance on Equipment activation to not use charge. ";
    protected override string Description => SimpleDesc + "Fractures Gesture of the Drowned".Style("#D2B088") + ".";
    public static string SimpleDesc = string.Format(
        "Activating your Equipment has a " + "{0}% ".Style(FontColor.cIsUtility) + "chance to " + "not use charge".Style(FontColor.cIsUtility) + ". ",
        Percent_Chance.Value
    );

    protected override string Lore => "The tranquil murmur of the waters echo as a reminder. Their kin remains.\r\n\r\n" +
        "This shell, passed from one to another. Carrying the memories of lost remnants, and new experiences - their hopes, their struggles, their legacy.\r\n\r\n" +
        "Would you allow this cycle to flow, or will you claim it as your own?";

    protected override bool IsEnabled()
    {
        Percent_Chance = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Chance to Not Charge", 35,
            new ConfigDescription(
                "[ 35 = 35% Chance | to not use Charge ]",
                new AcceptableValueRange<int>(1, 100)
            )
        );
        Item_Enabled = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Enable Rework", true,
            "[ True = Reworked | False = Vanilla ]"
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
    }
    private void PairFractured()
    {
        PurifiedTier.ItemCounterpartPool.Add(new PurifiedTier.PurifiedFractureInfo
        {
            purifiedItem = ItemDef.itemIndex,
            originalItem = ItemCatalog.FindItemIndex(GestureDrownedRework.StaticInternal)
        });
    }
}