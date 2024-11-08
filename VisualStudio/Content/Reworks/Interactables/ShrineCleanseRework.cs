using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace LunarsOfExiguity;
public class ShrineCleanseRework
{
    public static string StaticInternal = "Cleansing Pool";

    public static ConfigEntry<int> Irradiant_Chance;
    public static ConfigEntry<PoolSpawnType> SpawnType;
    public static ConfigEntry<int> PoolNValue;
    public static ConfigEntry<int> Use_Amount;

    public static int StageCounter;

    public enum PoolSpawnType
    {
        EveryNStage,
        AfterNStages,
        DirectorCost
    }

    private InteractableSpawnCard CleansePoolCard;
    public ShrineCleanseRework()
    {
        CreateConfig();

        CleansePoolCard = Addressables.LoadAsset<InteractableSpawnCard>("RoR2/Base/ShrineCleanse/iscShrineCleanse.asset").WaitForCompletion();

        if (SpawnType.Value != PoolSpawnType.DirectorCost) CleansePoolCard.directorCreditCost = int.MaxValue;
        else CleansePoolCard.directorCreditCost *= PoolNValue.Value;

        LanguageAPI.Add("CLEANSE_POOL_SPAWN", "A Cleansing Pool has surfaced..".Style("#D2B088"));

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
        SpawnType = LoEPlugin.Instance.Config.Bind(
            StaticInternal,
            "Spawn Type", PoolSpawnType.EveryNStage,
            "[ EveryNStage = Every Stage 1 - 5 in a Loop has a Pool | AfterNStages = After N Stage spawn a Pool | DirectorCost = Lower Spawns Frequently, Higher Spawns Less Frequently ]"
        );
        PoolNValue = LoEPlugin.Instance.Config.Bind(
            StaticInternal,
            "Pool N Variable", 5,
            "[ 5 = Spawn Every 5th Stage | 2 = Spawn After 2 Stages | 1 = 1x to Vanilla Director Cost ]"
        );
        Use_Amount = LoEPlugin.Instance.Config.Bind(
            StaticInternal,
            "Available Uses", 1,
            "[ 1 = 1 Time | before Cleansing Pool Disappears ]"
        );
    }

    private void ForceCleansePool(SceneDirector scene)
    {
        if (Run.instance)
        {
            int currentStage = Run.instance.stageClearCount + 1;
            bool spawnPool = false;

            if (SpawnType.Value == PoolSpawnType.EveryNStage && (currentStage - PoolNValue.Value) % 5 == 0) spawnPool = true;
            if (SpawnType.Value == PoolSpawnType.AfterNStages && currentStage % PoolNValue.Value == 0) spawnPool = true;

            if ((SceneInfo.instance.countsAsStage || SceneInfo.instance.sceneDef.allowItemsToSpawnObjects) && spawnPool)
            {
                Xoroshiro128Plus newRNG = new(scene.rng.nextUlong);
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = "CLEANSE_POOL_SPAWN" });
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(CleansePoolCard, new() { placementMode = DirectorPlacementRule.PlacementMode.Random }, newRNG));
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

        PoolUseCount count = context.purchasedObject.GetComponent<PoolUseCount>() ?? context.purchasedObject.AddComponent<PoolUseCount>();
        if (count)
        {
            count.Uses++;
            if (count.Uses >= Use_Amount.Value) context.purchasedObject.SetActive(false);
        }
    }

    public class PoolUseCount : NetworkBehaviour
    {
        [SyncVar]
        public int Uses;
    }
}
