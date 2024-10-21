using BepInEx.Configuration;

namespace LunarsOfExiguity;
public static class LoEConfig
{

    public static ConfigEntry<bool> Enable_Logging;
    public static ConfigEntry<RewriteOptions> Rework_Name;
    public static ConfigEntry<int> Round_To;

    public enum RewriteOptions
    {
        Relic,
        Cursed
    }
    public static void Init()
    {
        GeneralInit();
    }

    private static void GeneralInit()
    {
        string token = "! General !";
        Enable_Logging = LoEPlugin.Instance.Config.Bind(
            token, "Enable Logs", true,
            "[ True = Enables Logging | False = Disables Logging ]\nDisclaimer: Makes debugging harder when disabled"
        );
        Rework_Name = LoEPlugin.Instance.Config.Bind(
            token, "Relic Names", RewriteOptions.Relic,
            "[ Changes the naming conventions of Lunars | Does not effect 'Disables ...' ]"
        );
        ShrineCleanseRework.Irradiant_Chance = LoEPlugin.Instance.Config.Bind(
            token,
            "Chance for Irradiant Pearl", 20,
            new ConfigDescription(
                "[ 20 = 20% Chance | for Irradiant Pearl when using Cleansing Pool on a Lunar without a Purified item ]",
                new AcceptableValueRange<int>(1, 100)
            )
        );
        Round_To = LoEPlugin.Instance.Config.Bind(
            token, "Item Stat Rounding", 0,
            "[ 0 = Whole | 1 = Tenths | 2 = Hundrenths | 3 = ... ]\nRounds item values to respective decimal point"
        );
    }
}