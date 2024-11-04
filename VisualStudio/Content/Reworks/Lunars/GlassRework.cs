using BepInEx.Configuration;
using R2API;

using static LoEUtils;
using static LoEColors;
using RoR2;

namespace LunarsOfExiguity;
public class GlassRework : ItemReworkBase
{
    public static ConfigEntry<bool> Rework_Enabled;
    public static ConfigEntry<float> Permanent_Modifier;
    public static ConfigEntry<float> Damage_Modifier;
    public static ConfigEntry<float> Exponent_Coefficient;

    protected override string Name => "LunarDagger";
    public static readonly string StaticInternal = "LunarDagger";

    protected override string RelicNameOverride => "Relic of Glass";
    protected override string CursedNameOverride => RelicNameOverride;

    protected override string PickupOverride => "Take permanent damage".Style(FontColor.cDeath) + ". Increase damage for permanent damage taken.";
    protected override string DescriptionOverride => string.Format(
        "Take " + "permanent damage".Style(FontColor.cIsHealth) + ". Increase base damage by " + "500% ".Style(FontColor.cIsDamage) + "of " + "permanent damage ".Style(FontColor.cIsHealth) + "taken.",
        RoundVal(Permanent_Modifier.Value), RoundVal(Damage_Modifier.Value)
    );

    protected override bool IsEnabled()
    {
        Permanent_Modifier = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Permnanent Conversion", 100f,
            new ConfigDescription(
                "[ 100 = 100% Damage | is Permanent Damage ]",
                new AcceptableValueRange<float>(0, 100)
            )
        );
        Damage_Modifier = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Damage Multiplier", 500f,
            "[ 500 = 500% Base Damage | of Permanent Damage taken ]"
        );
        Exponent_Coefficient = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Exponent Coefficient", 1.75f,
            "[ 1.75 = 1.75 Exponential Coefficient | for Damage Multipler. Will automatically be put higher than 1. ]"
        );
        Rework_Enabled = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Enable Rework", true,
            "[ True = Reworked | False = Vanilla ]"
        );

        if (Exponent_Coefficient.Value <= 1f) Exponent_Coefficient.Value = 1.1f;

        return Rework_Enabled.Value;
    }
    /*
    protected override void Initialize()
    {
        if (PureGestureItem.Item_Enabled.Value)
        {
            LanguageAPI.AddOverlay(PureGestureItem.ItemDef.pickupToken, string.Format(
                PureGestureItem.SimplePickup + "Fractures {0}".Style("#D2B088") + ".",
                LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
            ));

            LanguageAPI.AddOverlay(PureGestureItem.ItemDef.descriptionToken, string.Format(
                PureGestureItem.SimpleDesc + "Fractures {1}".Style("#D2B088") + ".",
                RoundVal(PureGestureItem.Percent_Chance.Value),
                LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
            ));
        }
    }
    */
}