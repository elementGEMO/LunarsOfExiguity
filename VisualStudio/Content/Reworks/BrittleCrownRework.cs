using BepInEx.Configuration;
using R2API;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class BrittleCrownRework : ItemReworkBase
{
    public static ConfigEntry<bool> Rework_Enabled;
    public static ConfigEntry<float> Debt_Damage;

    protected override string Name => "GoldOnHit";
    public static readonly string StaticInternal = "GoldOnHit";

    protected override string RelicNameOverride => "Relic of Greed";
    protected override string CursedNameOverride => "Relic of Debt";

    protected override string PickupOverride => "You can buy everything... " + "BUT you can go into debt, increasing damage taken".Style(FontColor.cDeath) + ".";
    protected override string DescriptionOverride => string.Format(
        "When making a gold purchase that's too expensive, the purchase is " + "completed".Style(FontColor.cIsUtility) + ", but you go into " + "debt".Style(FontColor.cIsHealth) + ". Increase damage taken by " + "{0}% ".Style(FontColor.cIsHealth) + "for " + "every gold owed ".Style(FontColor.cIsHealth) + "while in " + "debt".Style(FontColor.cIsHealth) + ". " + "Scales inversely over time".Style(FontColor.cIsUtility) + ".",
        RoundVal(Debt_Damage.Value).SignVal()
    );

    protected override bool IsEnabled()
    {
        Debt_Damage = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Debt Damage Percent", 1.0f,
            "[ 1 = +1% Damage | per 1 Gold in Debt ]"
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
        if (PureCrownItem.Item_Enabled.Value)
        {
            LanguageAPI.AddOverlay(PureCrownItem.ItemDef.pickupToken, string.Format(
                PureCrownItem.SimplePickup + "Fractures {0}".Style("#D2B088") + ".",
                LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
            ));

            LanguageAPI.AddOverlay(PureCrownItem.ItemDef.descriptionToken, string.Format(
                PureCrownItem.SimpleDesc + "Fractures {1}".Style("#D2B088") + ".",
                RoundVal(PureCrownItem.Chance_Free.Value),
                LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
            ));
        }
    }
}