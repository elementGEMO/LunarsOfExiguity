using RoR2;
using UnityEngine;

namespace LunarsOfExiguity;
public class FocusCounterBuff : BuffBase
{
    protected override string Name => "RelicFocusCounter";
    public static BuffDef BuffDef;

    protected override Sprite IconSprite => LoEPlugin.Bundle.LoadAsset<Sprite>("FocusCounterIcon");
    protected override Color Color => new Color32(255, 86, 131, 255);
    protected override bool IsStackable => true;

    protected override void Initialize() => BuffDef = Value;
}