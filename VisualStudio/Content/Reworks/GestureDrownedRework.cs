using System;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

using static LoEUtils;
using static LoEColors;
using R2API;

namespace LunarsOfExiguity;
public class GestureDrownedRework : ItemReworkBase
{
    public static ConfigEntry<bool> Rework_Enabled;
    public static ConfigEntry<float> Base_Equip_Percent;
    public static ConfigEntry<float> Max_Equip_Percent;

    protected override string Name => "AutoCastEquipment";

    protected override string RelicNameOverride => "Relic of the Drowned";
    protected override string CursedNameOverride => RelicNameOverride;

    protected override string PickupOverride => "Equipments no longer use charge... " + "BUT activating your Equipment disables all skills temporarily".Style(FontColor.cDeath) + ".";//"Equipment use requires no charges... <style=cDeath>BUT activation disables all skills temporarily</style>.";
    protected override string DescriptionOverride => string.Format(
        "Equipments no longer use charge".Style(FontColor.cIsUtility) + ". Activating your Equipment temporarily " + "disables all skills ".Style(FontColor.cIsHealth) + "for " + "{0}% ".Style(FontColor.cIsHealth) + "of the " + "Equipment cooldown ".Style(FontColor.cIsUtility) + "on " + "each use".Style(FontColor.cIsHealth) + ", up to a " + "maximum ".Style(FontColor.cIsHealth) + "of " + "{1}%".Style(FontColor.cIsHealth) + ".",
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
        LanguageAPI.AddOverlay(PureGestureItem.ItemDef.descriptionToken, string.Format(
            PureGestureItem.SimpleDesc + "Fractures {1}".Style("#D2B088") + ".",
            PureGestureItem.Percent_Chance.Value, LoEConfig.Rework_Name.Value == LoEConfig.RewriteOptions.Relic ? RelicNameOverride : CursedNameOverride
        ));
    }
}