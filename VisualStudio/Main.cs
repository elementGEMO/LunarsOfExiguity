using BepInEx;
using RoR2;
using R2API;
using System.IO;
using UnityEngine;
//using ShaderSwapper;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace LunarsOfExiguity
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(ColorsAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "noodlegemo";
        public const string PluginName = "LunarsOfExiguity";
        public const string PluginVersion = "1.0.0";
        public void Awake()
        {
            MainConfig.SetUp(this);
            if (MainConfig.EnableLogs.Value) Log.Init(Logger);
            new AssetStatics(this);

            //StartCoroutine(AssetStatics.bundle.UpgradeStubbedShadersAsync());
            SetUpLunars();
        }
        private void SetUpLunars()
        {
            new Fractured();
            new GestureOfTheDrowned(GestureOfTheDrowned.Enable_Rework.Value);
            new FocusedConvergence();
        }
        /*
        private void SetUpPurified()
        {
            new PurifiedTier();
            //ItemCatalog.availability.onAvailable += TempAdd;
            //ItemCatalog.GetItemDef(RoR2Content.Items.Pearl.itemIndex).deprecatedTier = PurifiedTier.PurifiedTierDef.tier;
            //new PurifiedDrowned();
        }
        private void TempAdd()
        {
            ItemCatalog.GetItemDef(RoR2Content.Items.Pearl.itemIndex)._itemTierDef = PurifiedTier.PurifiedTierDef;
        }
        */
    }
    public class AssetStatics
    {
        public static readonly string tokenPrefix = "GEMO_LOE_";
        public static AssetBundle bundle;
        public static BaseUnityPlugin plugin;

        public AssetStatics(BaseUnityPlugin plugin)
        {
            AssetStatics.plugin = plugin;
            bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Directory.GetParent(plugin.Info.Location).ToString(), "lunarofexiguity"));
        }
    }
}