using RoR2;
using UnityEngine;

namespace LunarsOfExiguity;
public class DebtCountBuff : BuffBase
{
    protected override string Name => "DebtCountBuff";
    public static BuffDef BuffDef;

    protected override bool IsStackable => true;

    protected override void Initialize() => BuffDef = Value;
}