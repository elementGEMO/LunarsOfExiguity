using RoR2;
using UnityEngine;

using static LoEColorRegister;

namespace LunarsOfExiguity;
internal static class Colors
{
    public static ColorCatalog.ColorIndex TempPureLight { get; private set; }
    public static ColorCatalog.ColorIndex TempPureDark { get; private set; }
    public static void Init()
    {
        TempPureLight = RegisterColor(new Color32(210, 176, 136, 255));
        TempPureDark = RegisterColor(new Color32(173, 146, 108, 255));
    }
}