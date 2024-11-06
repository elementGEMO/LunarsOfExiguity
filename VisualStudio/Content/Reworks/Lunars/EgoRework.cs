using BepInEx.Configuration;
using R2API;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class EgoRework : ItemReworkBase
{
    public static ConfigEntry<bool> Rework_Enabled;

    protected override string Name => "LunarSun";
    public static readonly string StaticInternal = "LunarSun";

    protected override string RelicNameOverride => "Relic of Ego";
    protected override string CursedNameOverride => RelicNameOverride;

    protected override string PickupOverride => "...";
    protected override string DescriptionOverride => "...";

    protected override bool IsEnabled()
    {
        Rework_Enabled = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Enable Rework", true,
            "[ True = Reworked | False = Vanilla ]"
        );

        return Rework_Enabled.Value;
    }
    /*
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
    */
}