using BepInEx.Configuration;
using R2API;

using static LoEUtils;
using static LoEColors;
using RoR2;

namespace LunarsOfExiguity;
public class GestureDrownedRework : ItemReworkBase
{
    public static ConfigEntry<bool> Rework_Enabled;
    public static ConfigEntry<float> Base_Equip_Percent;
    public static ConfigEntry<float> Max_Equip_Percent;

    protected override string Name => "AutoCastEquipment";
    public static readonly string StaticInternal = "AutoCastEquipment";

    protected override string RelicNameOverride => "Relic of the Drowned";
    protected override string CursedNameOverride => RelicNameOverride;

    protected override string PickupOverride => "Equipments no longer use charge... " + "BUT activating your Equipment disables all skills temporarily".Style(FontColor.cDeath) + ".";
    protected override string DescriptionOverride => string.Format(
        "Equipments no longer use charge".Style(FontColor.cIsUtility) + ". Activating your Equipment temporarily " + "disables all skills ".Style(FontColor.cIsHealth) + "for " + "{0}% ".Style(FontColor.cIsHealth) + "of the " + "Equipment cooldown ".Style(FontColor.cIsUtility) + "for " + "each use".Style(FontColor.cIsHealth) + ", up to a " + "maximum ".Style(FontColor.cIsHealth) + "of " + "{1}%".Style(FontColor.cIsHealth) + ".",
        RoundVal(Base_Equip_Percent.Value), RoundVal(Max_Equip_Percent.Value)
    );

    protected override bool IsEnabled()
    {
        Base_Equip_Percent = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Percent Duration", 15f,
            "[ 15 = 15% Duration | Per Equipment Use ]"
        );
        Max_Equip_Percent = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Max Percent", 100f,
            "[ 100 = 100% Max Duration | Per Equipment Use]"
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
        LanguageAPI.AddOverlay(PureGestureItem.ItemDef.pickupToken, string.Format(
            PureGestureItem.SimplePickup + "Fractures {0}".Style("#D2B088") + ".",
            LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
        ));

        LanguageAPI.AddOverlay(PureGestureItem.ItemDef.descriptionToken, string.Format(
            PureGestureItem.SimpleDesc + "Fractures {0}".Style("#D2B088") + ".",
            LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
        ));
    }
}