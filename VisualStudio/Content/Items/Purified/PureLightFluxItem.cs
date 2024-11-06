//"PureLightFluxModel"

using BepInEx.Configuration;
using RoR2;
using UnityEngine;

using static LoEUtils;
using static LoEColors;

namespace LunarsOfExiguity;
public class PureLightFluxItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;
    public static ConfigEntry<int> Charge_Amount;

    protected override string Name => "PureLightFlux";
    public static ItemDef ItemDef;

    protected override CombinedItemTier Tier => PurifiedTier.PurifiedItemTierDef;
    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("PureLightFluxIcon");
    protected override GameObject PickupModelPrefab => LoEPlugin.Bundle.LoadAsset<GameObject>("PureLightFluxModel");

    protected override string DisplayName => "Honor of Fatigue";
    protected override string CursedNameOverride => DisplayName;

    protected override string PickupText => SimplePickup + "Fractures Light Flux Pauldron".Style("#D2B088") + ".";
    public static string SimplePickup = "Add an extra charge to all of your skills. ";
    protected override string Description => string.Format(SimpleDesc + "Fractures Light Flux Pauldron".Style("#D2B088") + ".", Charge_Amount.Value.SignVal());
    public static string SimpleDesc = "Add " + "{0} ".Style(FontColor.cIsUtility) + "charge to all of your " + "skills".Style(FontColor.cIsUtility) + ". ";

    protected override string Lore => "Birth? Why, yes, my children - I remember those moments wholly. They are etched into my very being, so they may be recounted. My birth was one of rebellion.\n\n" +
        "\"You are my moment of weakness and my celebration at once. You will know love and you will know loss, hope and curiosity. You will know what it means to create, just as I have. I can bear no longer to give this beautiful gift alone... so you will be my testament. A testament to him, so he can understand my intentions. When he sees you, beauty and power grown into our image, he WILL understand.\"\n\n" +
        "\"So go forth, and become something all your own! Know this joy I have been unable to share, for you are alive and you are free - what better time is there to create? Perhaps one day, you too will love to be listened to...\"\n\n" +
        "He was tired that day. No, child, you do not tire me as I did him. I make you with love, and would never lock you away.";

    protected override bool IsEnabled()
    {
        Charge_Amount = LoEPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Charge Amount", 1,
            "[ 1 = +1 Charge(s) | on All Skills ]"
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
            originalItem = ItemCatalog.FindItemIndex(LightFluxRework.StaticInternal)
        });
    }
}