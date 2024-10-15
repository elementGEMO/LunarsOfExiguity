using BepInEx;
using RoR2;
using R2API;
using System.IO;
using LunarsOfExiguity.Content.Buffs;
using LunarsOfExiguity.Content.Items;
using LunarsOfExiguity.Content.ItemTiers;
using LunarsOfExiguity.Content.Lunar.Reworks;
using LunarsOfExiguity.Content.Reworks;
using UnityEngine;
//using ShaderSwapper;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace LunarsOfExiguity
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(ColorsAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class LunarsOfExiguityPlugin : BaseUnityPlugin
    {
        public static readonly string TokenPrefix = "GEMO_LOE_";
        
        public static LunarsOfExiguityPlugin Instance { get; private set; }
        public static AssetBundle AssetBundle { get; private set; }
        
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "noodlegemo";
        public const string PluginName = "LunarsOfExiguity";
        public const string PluginVersion = "1.0.0";

        private void Awake() => Instance = this;

        private void Start()
        {
            LunarsOfExiguityConfig.Init();
            
            if (LunarsOfExiguityConfig.EnableLogging.Value) Log.Init(Logger);
    
            SetupAssets();
            SetupContent();
        }

        private void SetupContent()
        {
            SetupBuffs();
            SetupItems();
            SetupReworks();
        }

        private void SetupBuffs()
        {
            new RelicDisableSkillsDebuff();
        }

        private void SetupItems()
        {
            new PurifiedTier();
            new FracturedItem();
        }

        private void SetupReworks()
        {
            new AutoCastEquipmentRework();
            new FocusedConvergenceRework();
        }

        private void SetupAssets() => AssetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Directory.GetParent(Info.Location)!.ToString(), "lunarofexiguity"));
    }
}