using RoR2;
using UnityEngine;
using BepInEx.Configuration;

using static LoEColors;

namespace LunarsOfExiguity;
public class BlemishedItem : ItemBase
{
    public static ConfigEntry<bool> Gain_Blemished;

    protected override string Name => "Blemished";
    public static ItemDef ItemDef;

    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("PureConsumedIcon");

    protected override ItemTag[] Tags => [ItemTag.CannotCopy, ItemTag.CannotSteal, ItemTag.CannotDuplicate];

    protected override string DisplayName => "Blemished";
    protected override string PickupText => "Your " + "Purified ".Style("#D2B088") + "items have been dulled.";

    protected override bool IsEnabled()
    {
        Gain_Blemished = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item", "Gain Blemished Item", true,
            "[ True = Gain 'Blemished' when a Purified is Blemished | False = Nothing Happens ]"
        );

        return true;
    }

    protected override void Initialize() => ItemDef = Value;
}