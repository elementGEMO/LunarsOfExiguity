using BepInEx;
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
    public class LoEPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "noodlegemo";
        public const string PluginName = "LunarsOfExiguity";
        public const string PluginVersion = "1.0.0";
        public static LoEPlugin Instance { get; private set; }
        public static AssetBundle Bundle { get; private set; }

        public static readonly string TokenPrefix = "GEMO_LOE_";
        public void Awake()
        {
            Instance = this;

            SetupAssets();
            LoEConfig.Init();
            if (LoEConfig.Enable_Logging.Value) Log.Init(Logger);

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
            new SkillDisableDebuff();
        }

        private void SetupItems()
        {
            new PurifiedTier();
            new FracturedItem();
            new PureGestureItem();
        }

        private void SetupReworks()
        {
            new GestureDrownedRework();
            new FocusedConvergenceRework();
        }
        private void SetupAssets() => Bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Directory.GetParent(Info.Location)!.ToString(), "lunarofexiguity"));
    }
}