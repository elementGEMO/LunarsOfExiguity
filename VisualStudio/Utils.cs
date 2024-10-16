using System;

using LunarsOfExiguity;
public static class LoEUtils
{
    public static string SignVal(this float value) => value >= 0 ? "+" + value : "-" + value;
    public static float RoundVal(float value) => MathF.Round(value, LoEConfig.Round_To.Value);
}
public static class LoEColors
{
    public static string Style(this string self, FontColor style) => "<style=" + style + ">" + self + "</style>";
    public enum FontColor
    {
        cStack,
        cIsDamage,
        cIsHealth,
        cIsUtility,
        cIsHealing,
        cDeath,
        cSub,
        cKeywordName,
        cIsVoid,
        cIsLunar
    };
}