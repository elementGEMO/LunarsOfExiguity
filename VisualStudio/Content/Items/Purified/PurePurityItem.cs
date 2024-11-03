//"PureLightFluxModel"

using BepInEx.Configuration;
using RoR2;
using UnityEngine;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class PurePurityItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;
    public static ConfigEntry<float> Stat_Modifier;
    public static ConfigEntry<float> Cooldown_Reduce;

    protected override string Name => "PureLunarBadLuck";
    public static ItemDef ItemDef;

    protected override CombinedItemTier Tier => PurifiedTier.PurifiedItemTierDef;
    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("PurePurityIcon");
    protected override GameObject PickupModelPrefab => LoEPlugin.Bundle.LoadAsset<GameObject>("PurePurityModel");

    protected override string DisplayName => "Honor of Purity";
    protected override string CursedNameOverride => DisplayName;

    protected override string PickupText => SimplePickup + "Fractures Purity".Style("#D2B088") + ".";
    public static string SimplePickup = "Increase ALL of your stats and reduce all skill cooldowns. ";
    protected override string Description => string.Format(SimpleDesc + "Fractures Purity".Style("#D2B088") + ".", RoundVal(Stat_Modifier.Value), RoundVal(Cooldown_Reduce.Value));
    public static string SimpleDesc = "ALL stats ".Style(FontColor.cIsUtility) + "are increased by " + "{0}%".Style(FontColor.cIsUtility) + ". All " + "skill cooldowns ".Style(FontColor.cIsUtility) + "are reduced by " + "{1}%".Style(FontColor.cIsUtility) + ". ";

    protected override string Lore => "The seed in the garden never seems to grow.\n" +
        "It looks like rock, absorbing the sun within.\n\n" +
        "Even so, I continue. I continue to give it all. My praise. My care. My soul.\n" +
        "It smells like wind, swaying the nothingness.\n\n" +
        "All my attention yet nothing in return - not to sprout a single fruit.\n" +
        "It tastes like sky, sweet but vile.\n\n" +
        "Until... the seed had gone.\n" +
        "It feels like joy, spread amongst the populace.\n\n" +
        "I can only imagine the fruit it bore.\n" +
        "It sounded like two brothers, divided by vision.";

    protected override bool IsEnabled()
    {
        Stat_Modifier = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Charge Amount", 50f,
            "[ 50 = +50% Stat | Multiplier on ALL Stats ]"
        );
        Cooldown_Reduce = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Cooldown Reduction", 20f,
            new ConfigDescription(
                "[ 10 = 10% Cooldown Reduction | on All Skills ]",
                new AcceptableValueRange<float>(0, 100)
            )
        );
        Item_Enabled = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Enable Item", true,
            "[ True = Enabled | False = Disabled ]"
        );

        return Item_Enabled.Value;
    }

    protected override void Initialize()
    {
        ItemDef = Value;
        PurifiedTier.ItemTierPool.Add(ItemDef.itemIndex);
        ItemCatalog.availability.onAvailable += PairFractured;
    }
    protected override void LogDisplay()
    {
        ModelPanelParameters modelParam = PickupModelPrefab.AddComponent<ModelPanelParameters>();
        var foundMesh = PickupModelPrefab.transform.GetChild(0);

        if (!foundMesh) return;

        modelParam.focusPointTransform = foundMesh;
        modelParam.cameraPositionTransform = foundMesh;
        modelParam.minDistance = 0.05f;
        modelParam.maxDistance = 0.25f;
        modelParam.modelRotation = new Quaternion(0.0115291597f, -0.587752283f, 0.0455321521f, -0.807676435f);

        PickupModelPrefab.AddComponent<FloatingPointFix>();
    }

    private void PairFractured()
    {
        PurifiedTier.ItemCounterpartPool.Add(new PurifiedTier.PurifiedFractureInfo
        {
            purifiedItem = ItemDef.itemIndex,
            originalItem = ItemCatalog.FindItemIndex(PurityRework.StaticInternal)
        });
    }
}