using BepInEx;
using R2API;
using System.IO;
using UnityEngine;
using ShaderSwapper;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace LunarsOfExiguity
{
    // R2API Dependencies
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(ColorsAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]

    // Misc Dependencies
    //[BepInDependency("Goorakh-FreeCostFix-1.0.0", BepInDependency.DependencyFlags.SoftDependency)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class LoEPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "noodlegemo";
        public const string PluginName = "LunarsOfExiguity";
        public const string PluginVersion = "0.1.3";
        public static LoEPlugin Instance { get; private set; }
        public static AssetBundle Bundle { get; private set; }

        public static readonly string TokenPrefix = "GEMO_LOE_";
        public void Awake()
        {
            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };

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
            new PurifiedTier();
            new ShrineCleanseRework();

            new SkillDisableDebuff();
            new FocusCounterBuff();
            new DebtCountBuff();
            new StoneGrowthBuff();
            new GrowthDangerBuff();
        }
        private void SetUpItems()
        {
            new FracturedItem();
            new BlemishedItem();

            new GestureDrownedRework();
            new PureGestureItem();

            new FocusedConvergenceRework();
            new PureFocusItem();

            new BrittleCrownRework();
            new PureCrownItem();

            new LightFluxRework();
            new PureLightFluxItem();

            new StoneFluxRework();
            new PureStoneFluxItem();

            new PurityRework();
            new PurePurityItem();

            new GlassRework();
            new PureGlassItem();

            //new EgoRework();
        }
        private void SetUpMethods()
        {
            new ConsumeHooks();
            new CooldownHooks();

            new GestureDrownedHooks();
            new FocusConvergenceHooks();
            new BrittleCrownHooks();
            new LightFluxHooks();
            new StoneFluxHooks();
            new PurityHooks();
            new GlassHooks();
            //new EgoHooks();
        }
        private void SetupAssets()
        {
            Bundle = AssetBundle.LoadFromFile(Path.Combine(Directory.GetParent(Info.Location)!.ToString(), "lunarofexiguity"));
            StartCoroutine(Bundle.UpgradeStubbedShadersAsync());
        }
    }
}