using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class PurityRework : ItemReworkBase
{
    public static ConfigEntry<bool> Rework_Enabled;
    public static ConfigEntry<float> Stat_Modifier;
    public static ConfigEntry<float> Cooldown_Reduce;

    protected override string Name => "LunarBadLuck";
    public static readonly string StaticInternal = "LunarBadLuck";

    protected override string RelicNameOverride => "Relic of Purity";
    protected override string CursedNameOverride => RelicNameOverride;

    protected override string PickupOverride => "Increase ALL of your stats, and skills have NO cooldowns... " + "BUT you have no proc-coefficients".Style(FontColor.cDeath) + ".";
    protected override string DescriptionOverride => string.Format(
        "ALL stats ".Style(FontColor.cIsUtility) + "are increased by " + "{0}%".Style(FontColor.cIsUtility) + ". All " + "skill cooldowns ".Style(FontColor.cIsUtility) + "are reduced by " + "{1}%".Style(FontColor.cIsUtility) + ". " + "ALL proc-coefficients are set to 0".Style(FontColor.cIsHealth) + ".",
        RoundVal(Stat_Modifier.Value), RoundVal(Cooldown_Reduce.Value)
    );

    protected override bool IsEnabled()
    {
        Stat_Modifier = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Charge Amount", 150f,
            "[ 150 = +150% Stat | Multiplier on ALL Stats ]"
        );
        Cooldown_Reduce = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Cooldown Reduction", 100f,
            new ConfigDescription(
                "[ 100 = 100% Cooldown Reduction | on All Skills ]",
                new AcceptableValueRange<float>(0, 100)
            )
        );
        Rework_Enabled = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Enable Rework", true,
            "[ True = Reworked | False = Vanilla ]"
        );

        return Rework_Enabled.Value;
    }
    protected override void Initialize()
    {
        if (PurePurityItem.Item_Enabled.Value)
        {
            LanguageAPI.AddOverlay(PurePurityItem.ItemDef.pickupToken, string.Format(
                PurePurityItem.SimplePickup + "Fractures {0}".Style("#D2B088") + ".",
                LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
            ));

            LanguageAPI.AddOverlay(PurePurityItem.ItemDef.descriptionToken, string.Format(
                PurePurityItem.SimpleDesc + "Fractures {2}".Style("#D2B088") + ".",
                RoundVal(PurePurityItem.Stat_Modifier.Value), RoundVal(PurePurityItem.Cooldown_Reduce.Value),
                LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
            ));
        }
    }
}