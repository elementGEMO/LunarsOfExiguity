using RoR2;
using UnityEngine;

namespace LunarsOfExiguity;
public class StoneGrowthBuff : BuffBase
{
    protected override string Name => "StoneGrowth";
    public static BuffDef BuffDef;

    protected override Sprite IconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("StoneGrowthIcon");
    protected override Color Color => new Color32(68, 215, 236, 255);
    protected override bool IsStackable => true;
    protected override bool IsCooldown => true;

    protected override void Initialize() => BuffDef = Value;
}