using BepInEx;
using BepInEx.Configuration;

namespace LunarsOfExiguity;
public static class LoEConfig
{
    //public static ConfigEntry<int> RoundNumber;
    //public static ConfigEntry<bool> EnableLogs;
    //public static ConfigEntry<RewriteOptions> RelicNameRewrite;
    //public static ConfigEntry<FracturedOptions> FracturedCount;

    public static ConfigEntry<bool> Enable_Logging;
    public static ConfigEntry<RewriteOptions> Rework_Name;
    public static ConfigEntry<int> Round_To;

    public enum RewriteOptions
    {
        Vanilla,
        Relic,
        Cursed
    }
    public static void Init()
    {
        GeneralInit();
        //ReworkInit();
        //MiscInit();
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
            "[ Chanes the naming conventions of Lunars ]"
        );
        Round_To = LoEPlugin.Instance.Config.Bind(
            token, "Item Stat Rounding", 0,
            "[ 0 = Whole | 1 = Tenths | 2 = Hundrenths | 3 = ... ]\nRounds item values to respective decimal point"
        );
    }
    /*
    private static void ReworkInit()
    {
        // Gesture of the Drowned | Relic of the Drowned
        GestureRework.Enable_Rework = LoEPlugin.Instance.Config.Bind(
            GestureRework.Internal,
           "Enable Rework", true,
           "[ True = Reworked | False = Vanilla ]"
        );
        GestureRework.Base_Equip_Percent = LoEPlugin.Instance.Config.Bind(
            GestureRework.Internal,
            "Rework - Percent Duration", 15f,
            "[ 15 = 15% Duration | Per Equipment Use ]"
        );
        GestureRework.Max_Equip_Percent = LoEPlugin.Instance.Config.Bind(
            GestureRework.Internal,
            "Rework - Max Percent", 100f,
            "[ 100 = 100% Max Duration | Per Equipment Use]"
        );
    }
    */
    /*
    private static void MiscInit()
    {
        string fracturedToken = "Item - Fractured";
        FracturedItem.Fracture_Delay = LoEPlugin.Instance.Config.Bind(
            fracturedToken, "Fractured Delay", 0.5f,
            "[ 0.5 = 0.5s Fracture Notificaion Delay ]"
        );

        FracturedItem.Gain_Fracture = LoEPlugin.Instance.Config.Bind(
            fracturedToken, "Gain Fractured Item", true,
            "[ True = Gain 'Fractured' when a Lunar is Fractured | False = Nothing Happens ]"
        );
    }
    */
}

/*
namespace LunarsOfExiguity
{
    public static class Config
    {
        public static void SetUp()
        {
            GeneralConfig(plugin);
            LunarConfig(plugin);
        }

        public static void GeneralConfig(BaseUnityPlugin plugin)
        {
            string StaticName = "! General !";

            EnableLogs = plugin.Config.Bind(
                StaticName,
                "Enable Logs", true,
                "[ True = Keeps Logs | False = Turns Off Logs]\n If you disable this, I may not entirely be able to help you when an IL Hook fails"
            );

            RoundNumber = plugin.Config.Bind(
                StaticName,
                "Item Stats Round", 0,
                "[ 0 = Whole | 1 = Tenths | 2 = Hundrenths | 3 = ... ]\n Rounds item values to respective decimal spot"
            );

            RelicNameRewrite = plugin.Config.Bind(
                StaticName,
                "Relic Names", RewriteOptions.RelicRewrite,
                "Changing the naming conventions of Lunars"
            );

            FracturedCount = plugin.Config.Bind(
                StaticName,
                "Fractured Item", FracturedOptions.ItemOnFracture,
                "Whether to gain the 'Fractured' item to count Lunar conversions"
            );
        }
        public static void LunarConfig(BaseUnityPlugin plugin)
        {
            // Relic of the Drowned
            GestureOfTheDrowned.Enable_Rework = plugin.Config.Bind(
                GestureOfTheDrowned.Internal,
                "Enable Rework", true,
                "[ True = Reworked | False = Vanilla | Removed Stacking ]"
            );
            GestureOfTheDrowned.Base_Equip_Percent = plugin.Config.Bind(
                GestureOfTheDrowned.Internal,
                "Rework - Percent Duration", 15f,
                "[ 15 = 15% Duration | Per Equipment Use ]"
            );
            GestureOfTheDrowned.Max_Equip_Percent = plugin.Config.Bind(
                GestureOfTheDrowned.Internal,
                "Rework - Max Percent", 100f,
                "[ 100 = 100% Max Duration | Per Equipment Use]"
            );

            // Relic of Focus
            FocusedConvergence.Enable_Rework = plugin.Config.Bind(
                FocusedConvergence.Internal,
                "Enable Rework", true,
                "[ True = Reworked | False = Vanilla | Removed Stacking ]"
            );
            FocusedConvergence.Charge_Speed_Percent = plugin.Config.Bind(
                FocusedConvergence.Internal,
                "Rework - Charge Speed", 100f,
                "[ 100 = 100% Charge Speed | Teleporter Charge Speed Increase ]"
            );
            FocusedConvergence.Max_Damage_Percent = plugin.Config.Bind(
                FocusedConvergence.Internal,
                "Rework - Health Damage", 100,
                new ConfigDescription(
                    "[ 100 = 100% Health Damage | Against Enemies At 99% Teleporter Charge ]",
                    new AcceptableValueRange<int>(0, 100)
                )
            );
            FocusedConvergence.Percent_Loss_Hit = plugin.Config.Bind(
                FocusedConvergence.Internal,
                "Rework - Percent Loss", 25,
                new ConfigDescription(
                    "[ 25 = 25% Percent Loss | On Health Damage Per Hit Recieved ]",
                    new AcceptableValueRange<int>(0, 100)
                )
            );
        }

        public static ConfigEntry<int> RoundNumber;
        public static ConfigEntry<bool> EnableLogs;
        public static ConfigEntry<RewriteOptions> RelicNameRewrite;
        public static ConfigEntry<FracturedOptions> FracturedCount;

        public enum FracturedOptions
        {
            ItemOnFracture,
            NoItemOnFracture
        }
        public enum RewriteOptions
        {
            Vanilla,
            RelicRewrite,
            CursedRewrite
        }
    }
}
*/