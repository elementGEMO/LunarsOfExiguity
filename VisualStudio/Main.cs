using BepInEx;
using R2API;
using System.IO;
using UnityEngine;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace LunarsOfExiguity
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "noodlegemo";
        public const string PluginName = "LunarsOfExiguity";
        public const string PluginVersion = "1.0.0";
        public void Awake()
        {
            new AssetStatics(this);
            Log.Init(Logger);

            new Fractured();
            //SetUpLunars();
        }
        private void SetUpLunars()
        {

        }
    }
    public class AssetStatics
    {
        public static readonly string tokenPrefix = "GEMO_LOE_";
        public static AssetBundle bundle;
        public static BaseUnityPlugin plugin;

        public AssetStatics(BaseUnityPlugin plugin)
        {
            AssetStatics.plugin = plugin;
            bundle = AssetBundle.LoadFromFile(Path.Combine(Directory.GetParent(plugin.Info.Location).ToString(), "lunarofexiguity"));
        }
    }
}