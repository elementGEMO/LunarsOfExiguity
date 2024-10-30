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

    public static int Estimated_Max;

    protected override string Name => "HalfSpeedDoubleHealth";
    public static readonly string StaticInternal = "HalfSpeedDoubleHealth";

    protected override string RelicNameOverride => "Relic of Acromegaly";
    protected override string CursedNameOverride => RelicNameOverride;

    /*
    protected override string PickupOverride => "...";
    protected override string DescriptionOverride => string.Format(

    );
    */

    protected override bool IsEnabled()
    {
        Duration = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Duration", 15f,
            "[ 15 = 15 Seconds | of Item Duration ]"
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
        Rework_Enabled = LoEPlugin.Instance.Config.Bind(
            RelicNameOverride + " - Rework",
            "Enable Rework", true,
            "[ True = Reworked | False = Vanilla ]"
        );

        Estimated_Max = (int) Mathf.Round(Max_Size.Value / Size_Modifier.Value);

        return Rework_Enabled.Value;
    }
    /*
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
    */
}