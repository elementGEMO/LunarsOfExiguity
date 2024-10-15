using System;
using UnityEngine;

namespace LunarsOfExiguity;

public static class ItemUtils
{
    public static float RoundToValue(float x) => MathF.Round(x, MainConfig.RoundNumber.Value);
    
    public static string SignVal(this float value) => value >= 0 ? "+" + value : "-" + value;

}