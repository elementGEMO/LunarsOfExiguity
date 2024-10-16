using UnityEngine;

namespace LunarsOfExiguity;
public class PureGestureItem : ItemBase
{
    protected override string Name => "PurifiedAutoCastEquipment";
    protected override CombinedItemTier Tier => PurifiedTier.PurifiedItemTierDef;
    protected override Sprite PickupIconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("PureGestureIcon");
}