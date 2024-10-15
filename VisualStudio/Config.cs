using BepInEx;
using BepInEx.Configuration;

namespace LunarsOfExiguity
{
    public static class MainConfig
    {
        public static void SetUp(BaseUnityPlugin plugin)
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
            GestureOfTheDrowned.Enable_Rework = plugin.Config.Bind(
                GestureOfTheDrowned.Internal,
                "Enable Rework", true,
                "[ True = Reworked | False = Vanilla | Removed Stacking ]"
            );

            GestureOfTheDrowned.Base_Equip_Percent = plugin.Config.Bind(
                GestureOfTheDrowned.Internal,
                "Rework - Percent Duration", 15f,
                "[ 15.0f = 15% Duration ]\n Duration per Equipment use"
            );

            GestureOfTheDrowned.Max_Equip_Percent = plugin.Config.Bind(
                GestureOfTheDrowned.Internal,
                "Rework - Max Percent", 100f,
                "[ 100.0f = 100% Max Duration ]\n Max duration per Equipment use"
            );
        }

        public static ConfigEntry<NameStyle> ItemNameStyle;
        
        public static ConfigEntry<int> RoundNumber;
        public static ConfigEntry<bool> EnableLogs;
  
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

        public enum NameStyle
        {
            Vanilla,
            Relic,
            Cursed
        }
    }
}
