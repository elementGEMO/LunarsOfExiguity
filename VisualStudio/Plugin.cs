using BepInEx;
using R2API;
using System.IO;
using UnityEngine;
using ShaderSwapper;

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
            SetUpMisc();
            SetUpItems();
            SetUpMethods();
        }
        private void SetUpMisc()
        {
            //Colors.Init();
            new PurifiedTier();
            new SkillDisableDebuff();
            new ShrineCleanseRework();
        }
        private void SetUpItems()
        {
            new FracturedItem();
            new BlemishedItem();

            new GestureDrownedRework();
            new PureGestureItem();

            new FocusedConvergenceRework();
        }
        private void SetUpMethods()
        {
            new ConsumeHooks();
            new GestureDrownedHooks();
        }
        private void SetupAssets()
        {
            Bundle = AssetBundle.LoadFromFile(Path.Combine(Directory.GetParent(Info.Location)!.ToString(), "lunarofexiguity"));
            StartCoroutine(Bundle.UpgradeStubbedShadersAsync());
        }
    }
}