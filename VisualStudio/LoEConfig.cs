using BepInEx.Configuration;

namespace LunarsOfExiguity
{
    public static class LoEConfig
    {
        public static ConfigEntry<bool> EnableLogging;
        public static ConfigEntry<bool> GainItemOnFracture;
        public static ConfigEntry<float> FractureItemDelay;
        public static ConfigEntry<NameStyle> ItemNameStyle;
        public static ConfigEntry<int> RoundingLength;

        public static ConfigEntry<float> BaseDisableSkillsPercentage;
        public static ConfigEntry<float> MaxDisableSkillsPercentage;
        
        public static ConfigEntry<float> AdditionalChargeSpeedPercentage;
        public static ConfigEntry<float> MaxDamagePercentage;
        public static ConfigEntry<float> DamageLossOnHitPercentage;

        public static void Init()
        {
            GeneralConfig();
            ReworkConfig();
        }
        
        public static void GeneralConfig()
        {
            var section = "! General !";
            
            EnableLogging = LoEPlugin.Instance.Config.Bind(section, "Enable Logging", true, "[ True = Keeps Logs | False = Turns Off Logs]\n If you disable this, I may not entirely be able to help you when an IL Hook fails.");
            RoundingLength = LoEPlugin.Instance.Config.Bind(section, "Item Stats Round", 0, "[ 0 = Whole | 1 = Tenths | 2 = Hundredths | 3 = ... ]\n Rounds item values to respective decimal place.");
            ItemNameStyle = LoEPlugin.Instance.Config.Bind(section, "Relic Names", NameStyle.Relic, "The naming conventions of lunar items.");
            GainItemOnFracture = LoEPlugin.Instance.Config.Bind(section, "Gain Item on Fracture", true, "Whether to gain the 'Fractured' item to count lunar item conversions.");
            FractureItemDelay = LoEPlugin.Instance.Config.Bind(section, "Fracture Delay", 0.5f, "PLEASE WRITE SOMETHING FOR THIS GEMO I AM LAZY.");
        }
        
        public static void ReworkConfig()
        {
            BaseDisableSkillsPercentage = LoEPlugin.Instance.Config.Bind("AutoCastEquipment", "Rework - Percent Duration", 15f, "[ 15.0f = 15% Duration ]\n Duration per Equipment use");
            MaxDisableSkillsPercentage = LoEPlugin.Instance.Config.Bind("AutoCastEquipment", "Rework - Max Percent", 100f, "[ 100.0f = 100% Max Duration ]\n Max duration per Equipment use");

            // GEMO CHANGE THESE GEMO CHANGE THESE
            AdditionalChargeSpeedPercentage = LoEPlugin.Instance.Config.Bind("FocusConvergence", "Speed", 100f);
            MaxDamagePercentage = LoEPlugin.Instance.Config.Bind("FocusConvergence", "Max", 100f);
            DamageLossOnHitPercentage = LoEPlugin.Instance.Config.Bind("FocusConvergence", "Loss", 25f);
        }

        public enum NameStyle
        {
            Vanilla,
            Relic,
            Cursed
        }
    }
}
