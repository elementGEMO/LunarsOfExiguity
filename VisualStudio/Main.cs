using UnityEngine.Networking;
using BepInEx;
using R2API;
using RoR2;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace LunarsOfExiguity
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "noodlegemo";
        public const string PluginName = "LunarsOfExiguity";
        public const string PluginVersion = "1.0.0";
        public void Awake()
        {
            Log.Init(Logger);

            Inventory.onServerItemGiven += StopLunarStacking;
            SetUpLunars();
        }
        private void StopLunarStacking(Inventory inventory, ItemIndex itemIndex, int itemCount)
        {
            //Log.Debug(inventory.name);
            //Log.Debug(ItemCatalog.GetItemDef(item).name);
            //Log.Debug(what);

            if (!NetworkServer.active && inventory) return;

            ItemDef item = ItemCatalog.GetItemDef(itemIndex);
            CharacterMaster masterComponent = inventory.GetComponent<CharacterMaster>();
            if (!item || !masterComponent) return;

            if (item.tier == ItemTier.Lunar && inventory.GetItemCount(item) > 1)
            {
                //inventory.RemoveItem(itemIndex, itemCount - 1);
                inventory.RemoveItem(itemIndex, inventory.GetItemCount(item));
                CharacterMasterNotificationQueue.SendTransformNotification(masterComponent, itemIndex, (ItemIndex) 0, CharacterMasterNotificationQueue.TransformationType.LunarSun);
            }
        }
        private void SetUpLunars()
        {

        }
    }
}