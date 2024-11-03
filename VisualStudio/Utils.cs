using System;
using UnityEngine;
using System.Collections.Generic;
using RoR2;

using LunarsOfExiguity;

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

        return [.. renderInfos];
    }
}