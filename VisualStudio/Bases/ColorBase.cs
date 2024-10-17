using R2API;
using RoR2;
using UnityEngine;

namespace LunarsOfExiguity;
public abstract class ColorBase 
{
    public ColorCatalog.ColorIndex colorIndex;

    public abstract byte R { get; }

    public abstract byte G { get; }

    public abstract byte B { get; }

    public virtual string ColorCatalogEntryName { get; internal set; } = "DEFAULT";

    public abstract void Init();
    public void CreateColorCatalogEntry()
    {
        float floatRed = R / 255f;
        float floatGreen = G / 255f;
        float floatBlue = B / 255f;
        colorIndex = ColorsAPI.RegisterColor(new Color(floatRed, floatGreen, floatBlue));
    }
}