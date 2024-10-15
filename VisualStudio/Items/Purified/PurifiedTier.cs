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
        public class PurifiedTierDisplay : MonoBehaviour
        {
            public static void ModifyGenericPickup()
            {
                _pickup = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/GenericPickup.prefab").WaitForCompletion();

                var pickupDisplay = _pickup.transform.Find("PickupDisplay").gameObject;
                pickupDisplay.AddComponent<PurifiedTierDisplay>();
            }

            private static GameObject _pickup = null!;
            private PickupDisplay _display = null!;
            private bool _set;

            private void Awake()
            {
                _display = GetComponent<PickupDisplay>();
            }

            public void Update()
            {
                var pickupDef = PickupCatalog.GetPickupDef(_display.pickupIndex);
                if (pickupDef == null || _set) return;
                _set = true;
            }
        }
    }
}
