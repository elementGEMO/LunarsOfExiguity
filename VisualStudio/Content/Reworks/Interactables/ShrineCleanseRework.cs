using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LunarsOfExiguity;
public class ShrineCleanseRework
{
    public static string StaticInternal = "Cleansing Pool";

    public static ConfigEntry<int> Irradiant_Chance;
    public static ConfigEntry<int> Every_Stage;
    public static ConfigEntry<bool> Use_Once;

    private InteractableSpawnCard CleansePoolCard;
    public ShrineCleanseRework()
    {
        CreateConfig();

        CleansePoolCard = Addressables.LoadAsset<InteractableSpawnCard>("RoR2/Base/ShrineCleanse/iscShrineCleanse.asset").WaitForCompletion();
        CleansePoolCard.directorCreditCost = int.MaxValue;

        On.RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.PayCost += ReplaceWithPure;
        SceneDirector.onPrePopulateSceneServer += ForceCleansePool;
    }

    private void CreateConfig()
    {
        Irradiant_Chance = LoEPlugin.Instance.Config.Bind(
            StaticInternal,
            "Chance for Irradiant Pearl", 20,
            new ConfigDescription(
                "[ 20 = 20% Chance | for Irradiant Pearl when using Cleansing Pool on a Lunar without a Purified item ]",
                new AcceptableValueRange<int>(1, 100)
            )
        );
        Every_Stage = LoEPlugin.Instance.Config.Bind(
            StaticInternal,
            "Guaranteed on Stage", 5,
            "[ 5 = Every 5th | Stage a Cleansing Pool Spawns, doesn't spawn in any other Scenario ]"
        );
        Use_Once = LoEPlugin.Instance.Config.Bind(
            StaticInternal,
            "Availability", true,
            "[ True = Can be used once max | False = Can be used infinitely ]"
        );

        if (Every_Stage.Value <= 0) Every_Stage.Value = 1;
    }

    private void ForceCleansePool(SceneDirector scene)
    {
        if (Run.instance)
        {
            int currentStage = Run.instance.stageClearCount + 1;
            if (currentStage % Every_Stage.Value == 0)
            {
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(CleansePoolCard, new() { placementMode = DirectorPlacementRule.PlacementMode.Random }, scene.rng));
            }
        }
    }

    private void ReplaceWithPure(On.RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.orig_PayCost orig, CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
    {
        orig(costTypeDef, context);

        ShopTerminalBehavior selfShop = context.purchasedObject.GetComponent<ShopTerminalBehavior>();
        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/MonstersOnShrineUse"), new EffectData
        {
            origin = context.purchasedObject.transform.position,
            rotation = context.purchasedObject.transform.rotation,
        }, true);

        if (selfShop)
        {
            ItemIndex pureResult = ItemCatalog.FindItemIndex("Pearl"); ;
            if (Util.CheckRoll(Irradiant_Chance.Value)) pureResult = ItemCatalog.FindItemIndex("ShinyPearl");

            if (context.results.itemsTaken.Count > 0)
            {
                ItemIndex foundItem = context.results.itemsTaken[0];
                foreach (PurifiedTier.PurifiedFractureInfo pair in PurifiedTier.ItemCounterpartPool)
                {
                    bool sameIndex = foundItem == pair.originalItem;
                    if (sameIndex) { pureResult = pair.purifiedItem; break; }
                }
            }

            selfShop.SetPickupIndex(new PickupIndex(pureResult));
        }

        selfShop.enabled = false;
    }
}
