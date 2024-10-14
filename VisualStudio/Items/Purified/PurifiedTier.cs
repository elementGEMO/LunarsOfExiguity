using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LunarsOfExiguity
{
    public class PurifiedTier
    {
        public static ItemTierDef PurifiedTierDef;
        private static GameObject DropletPrefab;
        public PurifiedTier()
        {
            CreateDropletPrefab();

            PurifiedTierDef = ScriptableObject.CreateInstance<ItemTierDef>();
            PurifiedTierDef.name = "Purified";
            PurifiedTierDef.bgIconTexture = AssetStatics.bundle.LoadAsset<Sprite>("PurifiedBGIcon").texture;
            PurifiedTierDef.pickupRules = ItemTierDef.PickupRules.ConfirmFirst;
            PurifiedTierDef.canScrap = false;
            PurifiedTierDef.isDroppable = false;
            PurifiedTierDef.canRestack = false;
            PurifiedTierDef.colorIndex = ColorCatalog.ColorIndex.BossItem;
            PurifiedTierDef.darkColorIndex = ColorCatalog.ColorIndex.BossItemDark;
            PurifiedTierDef.dropletDisplayPrefab = DropletPrefab;
            PurifiedTierDef.tier = ItemTier.AssignedAtRuntime;
            
            ContentAddition.AddItemTierDef(PurifiedTierDef);
        }
        private void CreateDropletPrefab()
        {
            DropletPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/BossOrb.prefab").WaitForCompletion().InstantiateClone("PurifiedOrb", true);
        }
    }
}
