using BepInEx.Configuration;
using R2API;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class FocusedConvergenceRework : ItemReworkBase
{
    public static ConfigEntry<bool> Rework_Enabled;
    public static ConfigEntry<float> Charge_Speed_Percent;
    public static ConfigEntry<int> Max_Damage_Percent;
    public static ConfigEntry<int> Percent_Loss_Hit;

    protected override string Name => "FocusConvergence";
    public static readonly string StaticInternal = "FocusConvergence";

    protected override string RelicNameOverride => "Relic of Focus";
    protected override string CursedNameOverride => RelicNameOverride;

    protected override string PickupOverride => "Teleporter charges faster... " + "BUT all enemies are invincible".Style(FontColor.cDeath) + ", until after the event, taking damage instead.";
    protected override string DescriptionOverride => string.Format(
        "Teleporters charge " + "{0}% faster".Style(FontColor.cIsUtility) + ", but " + "enemies are invincible ".Style(FontColor.cIsHealth) + "during it. After the " + "Teleporter event".Style(FontColor.cIsUtility) + ", " + "enemies ".Style(FontColor.cIsHealth) + "lose " + "{1}% health".Style(FontColor.cIsHealth) + ", reduced by " + "{2}% ".Style(FontColor.cIsHealth) + "each time you were hit during it.",
        RoundVal(Charge_Speed_Percent.Value), Max_Damage_Percent.Value, Percent_Loss_Hit.Value
    );

    protected override bool IsEnabled()
    {
        Charge_Speed_Percent = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Charge Speed", 125f,
            "[ 125 = 125% Charge Speed | on Teleporters ]"
        );
        Max_Damage_Percent = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Health Loss", 100,
            new ConfigDescription(
                "[ 100 = 100% Health Loss | on Enemies at Teleporter Complete ]",
                new AcceptableValueRange<int>(0, 100)
            )
        );
        Percent_Loss_Hit = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Percent Loss", 25,
            new ConfigDescription(
                "[ 25 = 25% Percent Loss | on Health Damage per Hit Recieved ]",
                new AcceptableValueRange<int>(0, 100)
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
        if (PureFocusItem.Item_Enabled.Value)
        {
            LanguageAPI.AddOverlay(PureFocusItem.ItemDef.pickupToken, string.Format(
                PureFocusItem.SimplePickup + "Fractures {0}".Style("#D2B088") + ".",
                LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
            ));

            LanguageAPI.AddOverlay(PureFocusItem.ItemDef.descriptionToken, string.Format(
                PureFocusItem.SimpleDesc + "Fractures {2}".Style("#D2B088") + ".",
                PureFocusItem.Max_Damage_Percent.Value, PureFocusItem.Percent_Loss_Hit.Value,
                LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
            ));
        }
    }
}