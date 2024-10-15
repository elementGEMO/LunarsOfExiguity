using System;

namespace LunarsOfExiguity;

public static class LoEUtils
{
    public static float RoundToValue(float x) => MathF.Round(x, LoEConfig.RoundingLength.Value);
}