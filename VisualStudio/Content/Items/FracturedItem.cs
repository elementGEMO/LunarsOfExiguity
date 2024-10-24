using RoR2;
using UnityEngine;
using BepInEx.Configuration;

using static LoEColors;

namespace LunarsOfExiguity;
public class FracturedItem : ItemBase
{
    public static ConfigEntry<bool> Gain_Fracture;

    protected override string Name => "Fractured";
    public static ItemDef ItemDef;

    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("LunarConsumedIcon");

    protected override ItemTag[] Tags => [ItemTag.CannotCopy, ItemTag.CannotSteal, ItemTag.CannotDuplicate];

    protected override string DisplayName => "Fractured";
    protected override string PickupText => "Your " + "Lunar ".Style(FontColor.cIsLunar) + "items have shattered into pieces.";

    protected override bool IsEnabled()
    {
        Gain_Fracture = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item", "Gain Fractured Item", true,
            "[ True = Gain 'Fractured' when a Lunar is Fractured | False = Nothing Happens ]"
        );

        return true;
    }

    protected override void Initialize() => ItemDef = Value;
}