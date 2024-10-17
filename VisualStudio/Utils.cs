using System;

using LunarsOfExiguity;
using UnityEngine;
using System.Collections.Generic;
using RoR2;
public static class LoEUtils
{
    public static string SignVal(this float value) => value >= 0 ? "+" + value : "-" + value;
    public static float RoundVal(float value) => MathF.Round(value, LoEConfig.Round_To.Value);
}
public static class LoEColors
{
    public static string Style(this string self, FontColor style) => "<style=" + style + ">" + self + "</style>";
    public static string Style(this string self, string color) => "<color=" + color + ">" + self + "</color>";
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
public static partial class LoEColorRegister
{
    private static bool _hookEnabled = false;

    private static readonly List<Color32> indexToColor32 = [];
    private static readonly List<string> indexToHexString = [];
    internal static void SetHooks()
    {
        if (_hookEnabled)
        {
            return;
        }

        On.RoR2.ColorCatalog.GetColor += GetColor;
        On.RoR2.ColorCatalog.GetColorHexString += GetColorHex;

        _hookEnabled = true;
    }
    private static Color32 GetColor(On.RoR2.ColorCatalog.orig_GetColor orig, ColorCatalog.ColorIndex colorIndex)
    {
        if ((int)colorIndex < 0) return indexToColor32[-1 - ((int)colorIndex)];
        return orig(colorIndex);
    }
    private static string GetColorHex(On.RoR2.ColorCatalog.orig_GetColorHexString orig, ColorCatalog.ColorIndex colorIndex)
    {
        if ((int)colorIndex < 0) return indexToHexString[-1 - ((int)colorIndex)];
        return orig(colorIndex);
    }

    public static ColorCatalog.ColorIndex RegisterColor(Color color)
    {
        SetHooks();

        int nextColorIndex = -indexToColor32.Count - 1;
        ColorCatalog.ColorIndex newIndex = (ColorCatalog.ColorIndex)nextColorIndex;
        indexToColor32.Add(color);
        indexToHexString.Add(Util.RGBToHex(color));

        return newIndex;
    }
}