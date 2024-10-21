using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LunarsOfExiguity;
public abstract class ItemTierBase : GenericBase<ItemTierDef>
{
    protected virtual ColorCatalog.ColorIndex Color => ColorCatalog.ColorIndex.None;
    protected virtual ColorCatalog.ColorIndex DarkColor => ColorCatalog.ColorIndex.None;

    protected virtual GameObject HighlightPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();//Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/GenericPickup.prefab").WaitForCompletion();
    protected virtual GameObject DropletDisplayPrefab => Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/LunarOrb.prefab").WaitForCompletion();
    protected virtual Texture IconBackgroundTexture => null;

    protected virtual bool CanBeDropped => true;
    protected virtual bool CanBeScrapped => true;
    protected virtual bool CanBeRestacked => true;

    protected virtual ItemTierDef.PickupRules PickupRules => ItemTierDef.PickupRules.Default;

    protected override void Create()
    {
        Value = ScriptableObject.CreateInstance<ItemTierDef>();

        if (Value)
        {
            Value.name = Name;

            Value.colorIndex = Color;
            Value.darkColorIndex = DarkColor;

            Value.highlightPrefab = HighlightPrefab;
            Value.dropletDisplayPrefab = DropletDisplayPrefab;
            Value.bgIconTexture = IconBackgroundTexture;

            Value.isDroppable = CanBeDropped;
            Value.canScrap = CanBeScrapped;
            Value.canRestack = CanBeRestacked;

            Value.pickupRules = PickupRules;

            Value.tier = ItemTier.AssignedAtRuntime;
        }

        ContentAddition.AddItemTierDef(Value);
    }
}