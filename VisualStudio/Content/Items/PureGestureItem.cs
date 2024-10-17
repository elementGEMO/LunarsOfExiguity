using BepInEx.Configuration;
using RoR2;
using UnityEngine;

using static LoEColors;

namespace LunarsOfExiguity;
public class PureGestureItem : ItemBase
{
    protected override string Name => "PureAutoCastEquipment";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => PurifiedTier.PurifiedItemTierDef;
    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("PureGestureIcon");

    protected override string DisplayName => "Honor of the Drowned";
    protected override string CursedNameOverride => DisplayName;
    protected override string PickupText => "...";
    protected override string Description => string.Format(
        "Activating your Equipment has a " + "40% ".Style(FontColor.cIsUtility) + "chance to " + "not use charge".Style(FontColor.cIsUtility) + ". " + "Disables Relic of the Drowned".Style("#D2B088") + "."
    );
    protected override void Create()
    {
        PurifiedTier.ItemTierPool.Add(ItemDef);
    }
}