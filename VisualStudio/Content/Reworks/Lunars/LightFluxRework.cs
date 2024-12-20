﻿using BepInEx.Configuration;
using R2API;
using RoR2;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class LightFluxRework : ItemReworkBase
{
    public static ConfigEntry<bool> Rework_Enabled;
    public static ConfigEntry<int> Charge_Amount;
    public static ConfigEntry<float> Cooldown_Reduction;
    public static ConfigEntry<float> Attack_Speed_Percent;

    protected override string Name => "HalfAttackSpeedHalfCooldowns";
    public static readonly string StaticInternal = "HalfAttackSpeedHalfCooldowns";

    protected override string RelicNameOverride => "Relic of Fatigue";
    protected override string CursedNameOverride => RelicNameOverride;

    protected override string PickupOverride => "Add several charges to all of your skills, and reduce all skill cooldowns... " + "BUT attack slower with every charge missing".Style(FontColor.cDeath) + ".";
    protected override string DescriptionOverride => string.Format(
        "Add " + "{0} ".Style(FontColor.cIsUtility) + "charges to all of your " + "skills".Style(FontColor.cIsUtility) + ". " + "Reduces all skill cooldowns ".Style(FontColor.cIsUtility) + "by " + "{1}%".Style(FontColor.cIsUtility) + ". " + "Decrease attack speed ".Style(FontColor.cIsDamage) + "by " + "{2}% ".Style(FontColor.cIsDamage) + "with " + "every charge missing".Style(FontColor.cIsHealth) + ".",
        Charge_Amount.Value.SignVal(), RoundVal(Cooldown_Reduction.Value), RoundVal(Attack_Speed_Percent.Value)
    );

    protected override bool IsEnabled()
    {
        Charge_Amount = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Charge Amount", 4,
            "[ 4 = +4 Charge(s) | on All Skills ]"
        );
        Cooldown_Reduction = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Cooldown Reduction", 50f,
            new ConfigDescription(
                "[ 50 = 50% Cooldown Reduction | on All Skills ]",
                new AcceptableValueRange<float>(0, 100)
            )
        );
        Attack_Speed_Percent = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Attack Speed Reduction", 12.5f,
            new ConfigDescription(
                "[ 12.5 = 12.5% Attack Speed | Reduction per Charge Missing, scales Hyperbolically ]",
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
        if (PureLightFluxItem.Item_Enabled.Value)
        {
            LanguageAPI.AddOverlay(PureLightFluxItem.ItemDef.pickupToken, string.Format(
                PureLightFluxItem.SimplePickup + "Fractures {0}".Style("#D2B088") + ".",
                LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
            ));

            LanguageAPI.AddOverlay(PureLightFluxItem.ItemDef.descriptionToken, string.Format(
                PureLightFluxItem.SimpleDesc + "Fractures {1}".Style("#D2B088") + ".",
                PureLightFluxItem.Charge_Amount.Value.SignVal(),
                LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
            ));
        }
    }
}