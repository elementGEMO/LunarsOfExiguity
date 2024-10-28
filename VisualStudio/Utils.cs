using System;

using LunarsOfExiguity;
using UnityEngine;
using System.Collections.Generic;
using RoR2;

internal static class LoEUtils
{
    public static string SignVal(this float value) => value >= 0f ? "+" + value : "-" + value;
    public static string SignVal(this int value) => value >= 0 ? "+" + value : "-" + value;
    public static float RoundVal(float value) => MathF.Round(value, LoEConfig.Round_To.Value);
}
internal static class LoEColors
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
internal class LoEColorRegister
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
internal class LoERenderHelper
{
    public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject self)
    {
        Renderer[] allRender = self.GetComponentsInChildren<Renderer>();
        List<CharacterModel.RendererInfo> renderInfos = [];

        foreach (Renderer render in allRender)
        {
            renderInfos.Add(new CharacterModel.RendererInfo
            {
                defaultMaterial = render.sharedMaterial,
                renderer = render,
                defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                ignoreOverlays = false
            });
        }

        /*
        CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[allRender.Length];

        for (int i = 0; i < allRender.Length; i++)
        {
            renderInfos[i] = new CharacterModel.RendererInfo
            {
                defaultMaterial = allRender[i].sharedMaterial,
                renderer = allRender[i],
                defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                ignoreOverlays = false
            };
        }
        */

        return [.. renderInfos];
    }
}