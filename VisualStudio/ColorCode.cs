namespace LunarsOfExiguity
{
    public static class ColorCodeDISABLED
    {
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

        public static string Style(this string self, FontColor style) => "<style=" + style + ">" + self + "</style>";
    }
}