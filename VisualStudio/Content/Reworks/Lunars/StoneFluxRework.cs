using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class StoneFluxRework : ItemReworkBase
{
    public static ConfigEntry<bool> Rework_Enabled;
    public static ConfigEntry<float> Duration;
    public static ConfigEntry<float> Size_Modifier;
    public static ConfigEntry<float> Max_Size;
    public static ConfigEntry<int> Armor_Gain;
    public static ConfigEntry<float> Speed_Reduction;
    public static ConfigEntry<bool> Hyperbolic_Reduction;

    public static int Estimated_Max;

    protected override string Name => "HalfSpeedDoubleHealth";
    public static readonly string StaticInternal = "HalfSpeedDoubleHealth";

    protected override string RelicNameOverride => "Relic of Acromegaly";
    protected override string CursedNameOverride => RelicNameOverride;

    protected override string PickupOverride => "Taking damage causes you to grow, increasing armor and reducing speed.";
    protected override string DescriptionOverride => string.Format(
        "Taking damage increases your " + "size ".Style(FontColor.cIsUtility) + "by " + "{0}%".Style(FontColor.cIsUtility) + ", up to a " + "maximum ".Style(FontColor.cIsUtility) + "of " + "{1}% ".Style(FontColor.cIsUtility) + ", for " + "{2} ".Style(FontColor.cIsUtility) + "seconds. Increases " + "armor ".Style(FontColor.cIsHealing) + "by " + "{3} ".Style(FontColor.cIsHealing) + "and reduces " + "movement speed ".Style(FontColor.cIsHealth) + "by " + "{4}% ".Style(FontColor.cIsHealth) + "for every " + "{0}% ".Style(FontColor.cIsUtility) + "increase in " + "size".Style(FontColor.cIsUtility) + ".",
        RoundVal(Size_Modifier.Value), RoundVal(Max_Size.Value),
        RoundVal(Duration.Value),
        Armor_Gain.Value, RoundVal(Speed_Reduction.Value)
    );

    protected override bool IsEnabled()
    {
        Duration = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Duration", 10f,
            "[ 10 = 10 Seconds | of Item Duration ]"
        );
        Size_Modifier = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Size Modifier", 10f,
            "[ 10 = 10% Size Increase | per Buff / Hit taken ]"
        );
        Max_Size = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Max Size", 200f,
            "[ 200 = 200% Max Size | Value is Rounded to fit Size Mod ]"
        );
        Armor_Gain = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Armor Gain", 25,
            "[ 25 = +25 Armor | per Buff / Hit Taken ]"
        );
        Speed_Reduction = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Movement Speed Reduction", 5f,
            "[ 5 = 5% Speed | Reduced per Buff / Hit Taken ]"
        );
        Hyperbolic_Reduction = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Hyperbolic Scaling", true,
            "[ True = Hyperbolic Speed Reduction | False = Linear Speed Reduction ]"
        );
        Rework_Enabled = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Enable Rework", true,
            "[ True = Reworked | False = Vanilla ]"
        );

        Estimated_Max = (int) Mathf.Round(Max_Size.Value / Size_Modifier.Value);

        return Rework_Enabled.Value;
    }
    protected override void Initialize()
    {
        if (PureStoneFluxItem.Item_Enabled.Value)
        {
            LanguageAPI.AddOverlay(PureStoneFluxItem.ItemDef.pickupToken, string.Format(
                PureStoneFluxItem.SimplePickup + "Fractures {0}".Style("#D2B088") + ".",
                LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
            ));

            LanguageAPI.AddOverlay(PureStoneFluxItem.ItemDef.descriptionToken, string.Format(
                PureStoneFluxItem.SimpleDesc + "Fractures {2}".Style("#D2B088") + ".",
                Armor_Gain.Value, RoundVal(Size_Modifier.Value),
                LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
            ));
        }
    }
}