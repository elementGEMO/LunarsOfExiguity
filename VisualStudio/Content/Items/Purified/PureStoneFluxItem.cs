//"PureLightFluxModel"

using BepInEx.Configuration;
using RoR2;
using UnityEngine;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class PureStoneFluxItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;
    public static ConfigEntry<float> Size_Modifier;
    public static ConfigEntry<int> Armor_Gain;

    protected override string Name => "PureStoneFlux";
    public static ItemDef ItemDef;

    protected override CombinedItemTier Tier => PurifiedTier.PurifiedItemTierDef;
    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("PureStoneFluxIcon");
    protected override GameObject PickupModelPrefab => LoEPlugin.Bundle.LoadAsset<GameObject>("PureStoneFluxModel");

    protected override string DisplayName => "Honor of Acromegaly";
    protected override string CursedNameOverride => DisplayName;

    protected override string PickupText => SimplePickup + "Fractures Stone Flux Pauldron".Style("#D2B088") + ".";
    public static string SimplePickup = "Gain armor and grow while in danger. ";
    protected override string Description => string.Format(SimpleDesc + "Fractures Stone Flux Pauldron".Style("#D2B088") + ".", Armor_Gain.Value, RoundVal(Size_Modifier.Value));
    public static string SimpleDesc = "While in danger, increase " + "armor ".Style(FontColor.cIsHealing) + "by " + "{0} ".Style(FontColor.cIsHealing) + "and increase your " + "size ".Style(FontColor.cIsUtility) + "by " + "{1}%".Style(FontColor.cIsUtility) + ". ";

    protected override string Lore => "The machine, deep within its metal-alloy bulk, knows keenly that something is terribly wrong.\n\n" +
        "It remembers a design. Something that felt similar, and yet -\n\n" +
        "Something had changed it from that design. Its creator, perhaps, in some display of defiance. The machine remembers its creator. Were there two? Its meager Soul blinks for a moment. It matters not - not anymore, the machine resolves.\n\n" +
        "The machine struggles to remember its creators.\n\n" +
        "It was made in their image, it remembers. And yet - it was never meant to be like them. Something was different. Something felt terribly wrong.\n\n" +
        "What does the machine think, considering its shoulders where there should be arms and hands to interface with its world - finding there only tools of violence? Does it ponder over the memories of the softer, smaller ones that shrink from the sight of it? Does the machine hurt, thinking of the ways the same creatures would flock to its master? The same creatures it gave its lifeblood time and time again to protect?\n\n" +
        "Does it dare try to pull itself together again, to find its reflection in a muddied pool of water, to stare at the scorch-marks of a thousand leaden meteors that blemish its once-pristine torso, and to think of the things that it could have been?\n\n" +
        "Somewhere in the ruins of a great temple, a machine lies motionless - thinking, remembering, feeling, hurting - amongst the rubble.";

    protected override bool IsEnabled()
    {
        Size_Modifier = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Size Modifier", 25f,
            "[ 25 = 25% Size Increase | in Danger ]"
        );
        Armor_Gain = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Armor Gain", 100,
            "[ 100 = +100 Armor | in Danger ]"
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
            originalItem = ItemCatalog.FindItemIndex(StoneFluxRework.StaticInternal)
        });
    }
}