using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;

namespace LunarsOfExiguity;
public class StoneFluxHooks
{
    private static readonly string InternalName = "StoneFluxHooks";
    public static bool ReworkItemEnabled;
    public static bool PureItemEnabled;

    public StoneFluxHooks()
    {
        ReworkItemEnabled = StoneFluxRework.Rework_Enabled.Value;
        PureItemEnabled = PureStoneFluxItem.Item_Enabled.Value;

        if (ReworkItemEnabled || PureItemEnabled)
        {
            On.RoR2.CharacterBody.OnBuffFirstStackGained += GrowComponent;
        }
        if (ReworkItemEnabled)
        {
            IL.RoR2.CharacterBody.RecalculateStats += DisableEffect;
            GlobalEventManager.onServerDamageDealt += AddLunarSize;
            RecalculateStatsAPI.GetStatCoefficients += LunarStats;
        }
        if (PureItemEnabled)
        {
            RecalculateStatsAPI.GetStatCoefficients += PureStats;
            On.RoR2.CharacterBody.OnInventoryChanged += PureBehavior;
        }
    }

    private void GrowComponent(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
    {
        orig(self, buffDef);

        if (buffDef == StoneGrowthBuff.BuffDef || buffDef == GrowthDangerBuff.BuffDef)
        {
            if (!self.GetComponent<StoneSize>()) self.gameObject.AddComponent<StoneSize>();
        }
    }

    private void DisableEffect(ILContext il)
    {
        ILCursor cursor = new(il);
        int itemIndex = -1;

        cursor.TryGotoNext(
            x => x.MatchLdsfld(typeof(DLC1Content.Items), nameof(DLC1Content.Items.HalfSpeedDoubleHealth)),
            x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
            x => x.MatchStloc(out itemIndex)
        );

        if (itemIndex != -1 && cursor.TryGotoNext(x => x.MatchLdarg(0)))
        {
            cursor.Emit(OpCodes.Ldloc, itemIndex);
            cursor.EmitDelegate<Func<int, int>>(self => 0);
            cursor.Emit(OpCodes.Stloc, itemIndex);
        }
        else Log.Warning(InternalName + " - #1 (ReplaceStoneEffect) Failure");
    }
    private void AddLunarSize(DamageReport damageReport)
    {
        if (NetworkServer.active && damageReport.victimBody?.inventory)
        {
            int itemCount = damageReport.victimBody.inventory.GetItemCount(DLC1Content.Items.HalfSpeedDoubleHealth);
            int buffCount = damageReport.victimBody.GetBuffCount(StoneGrowthBuff.BuffDef);

            if (itemCount > 0)
            {
                int newBuffCount = Mathf.Min(buffCount + 1, StoneFluxRework.Estimated_Max);
                for (int i = 0; i < buffCount; i++) damageReport.victimBody.RemoveOldestTimedBuff(StoneGrowthBuff.BuffDef);
                for (int i = 0; i < newBuffCount; i++) damageReport.victimBody.AddTimedBuff(StoneGrowthBuff.BuffDef, StoneFluxRework.Duration.Value);
            }
        }
    }
    private void LunarStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (NetworkServer.active && sender)
        {
            int buffCount = sender.GetBuffCount(StoneGrowthBuff.BuffDef);
            if (buffCount > 0)
            {
                args.armorAdd += buffCount * StoneFluxRework.Armor_Gain.Value;

                float reduction = StoneFluxRework.Speed_Reduction.Value / 100f;
                if (StoneFluxRework.Hyperbolic_Reduction.Value) reduction = Util.ConvertAmplificationPercentageIntoReductionPercentage(reduction);

                args.moveSpeedMultAdd += buffCount * -reduction;
            }
        }
    }

    private void PureStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (NetworkServer.active && sender)
        {
            int buffCount = sender.GetBuffCount(GrowthDangerBuff.BuffDef);
            if (buffCount > 0)
            {
                args.armorAdd += PureStoneFluxItem.Armor_Gain.Value;
            }
        }
    }
    private void PureBehavior(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
    {
        orig(self); if (NetworkServer.active) self.AddItemBehavior<PureStoneBehavior>(self.inventory.GetItemCount(PureStoneFluxItem.ItemDef));
    }

    public class StoneSize : MonoBehaviour
    {
        private CharacterBody SelfBody;
        private CameraTargetParams SelfCamera;
        private Transform ModelTransform;

        private Vector3 BaseScale;
        private float GrowthMod;

        private CameraTargetParams.AimRequest AimRequest;
        private int CurrentAim;

        private void Awake()
        {
            SelfBody = GetComponent<CharacterBody>();
            ModelTransform = GetComponent<ModelLocator>()?.modelTransform;
            if (ModelTransform) BaseScale = ModelTransform.localScale;
            SelfCamera = GetComponent<CameraTargetParams>();
        }
        private void OnDestroy()
        {
            if (SelfBody && ModelTransform) ModelTransform.localScale = BaseScale;
        }

        private void LateUpdate()
        {
            if (!SelfBody || !NetworkServer.active) return;

            int buffCount = SelfBody.GetBuffCount(StoneGrowthBuff.BuffDef);
            bool hasPureBuff = SelfBody.GetBuffCount(GrowthDangerBuff.BuffDef) > 0;
            float finalGrowth = 0;

            if (buffCount > 0) finalGrowth += buffCount * StoneFluxRework.Size_Modifier.Value / 50f;
            if (hasPureBuff) finalGrowth += PureStoneFluxItem.Size_Modifier.Value / 50f;

            GrowthMod = Mathf.Lerp(GrowthMod, finalGrowth, Time.deltaTime * 4);

            if (ModelTransform) ModelTransform.localScale = BaseScale * (1f + GrowthMod);

            if (SelfCamera)
            {
                if (CurrentAim <= 0 && GrowthMod > 0.666)
                {
                    CurrentAim = 1;
                    AimRequest?.Dispose();
                    AimRequest = SelfCamera.RequestAimType(CameraTargetParams.AimType.Aura);
                }
                else if (CurrentAim <= 1 && GrowthMod > 1.333)
                {
                    CurrentAim = 2;
                    AimRequest?.Dispose();
                    AimRequest = SelfCamera.RequestAimType(CameraTargetParams.AimType.ZoomedOut);
                }
                else if (CurrentAim > 0)
                {
                    CurrentAim = 0;
                    AimRequest?.Dispose();
                }
            }
        }
    }
    private class PureStoneBehavior : CharacterBody.ItemBehavior
    {
        private void Awake() => enabled = false;

        private void FixedUpdate()
        {
            bool hasBuff = body.HasBuff(GrowthDangerBuff.BuffDef);
            bool inDanger = !body.outOfDanger;

            if (!hasBuff && inDanger) body.AddBuff(GrowthDangerBuff.BuffDef);
            else if (hasBuff && !inDanger) body.RemoveBuff(GrowthDangerBuff.BuffDef);
        }
    }
}