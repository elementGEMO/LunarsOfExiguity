using RoR2;
using UnityEngine;

namespace LunarsOfExiguity;
public class GrowthDangerBuff : BuffBase
{
    protected override string Name => "GrowthInDanger";
    public static BuffDef BuffDef;

    protected override Sprite IconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("StoneGrowthIcon");
    protected override Color Color => new Color32(207, 171, 134, 255);
    protected override bool IsStackable => false;

    protected override void Initialize() => BuffDef = Value;
}