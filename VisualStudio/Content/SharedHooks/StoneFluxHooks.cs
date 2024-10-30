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
    //public static bool PureItemEnabled;

    public StoneFluxHooks()
    {
        ReworkItemEnabled = StoneFluxRework.Rework_Enabled.Value;
        //PureItemEnabled = PureLightFluxItem.Item_Enabled.Value;
        if (ReworkItemEnabled)
        {
            CharacterBody.onBodyAwakeGlobal += AddGrowthBehavior;
        }
        if (ReworkItemEnabled)
        {
            IL.RoR2.CharacterBody.RecalculateStats += ReplaceStoneEffect;
            GlobalEventManager.onServerDamageDealt += IncreaseSize;
            RecalculateStatsAPI.GetStatCoefficients += SlowStats;
        }
    }

    private void SlowStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (NetworkServer.active && sender)
        {
            int buffCount = sender.GetBuffCount(StoneGrowthBuff.BuffDef);
            if (buffCount > 0)
            {
                args.armorAdd = buffCount * StoneFluxRework.Armor_Gain.Value;
                args.moveSpeedMultAdd = buffCount * -(StoneFluxRework.Speed_Reduction.Value / 100f);
            }
        }
    }

    private void AddGrowthBehavior(CharacterBody self) => self.gameObject.AddComponent<StoneSize>();
    private void ReplaceStoneEffect(ILContext il)
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
    private void IncreaseSize(DamageReport damageReport)
    {
        if (NetworkServer.active && damageReport.victimBody?.inventory)
        {
            int itemCount = damageReport.victimBody.inventory.GetItemCount(DLC1Content.Items.HalfSpeedDoubleHealth);
            int buffCount = damageReport.victimBody.GetBuffCount(StoneGrowthBuff.BuffDef);

            if (itemCount > 0)
            {
                damageReport.victimBody.AddTimedBuff(StoneGrowthBuff.BuffDef, StoneFluxRework.Size_Modifier.Value);
                if (buffCount >= StoneFluxRework.Estimated_Max) damageReport.victimBody.SetBuffCount(StoneGrowthBuff.BuffDef.buffIndex, StoneFluxRework.Estimated_Max);
            }
        }
    }

    public class StoneSize : MonoBehaviour
    {
        private CharacterBody SelfBody;
        private CameraTargetParams SelfCamera;
        private Transform ModelTransform;

        private Vector3 BaseScale;
        private float GrowthMod;

        private CameraTargetParams.AimRequest AimRequest;
        private bool HasAim;

        private void Awake()
        {
            SelfBody = GetComponent<CharacterBody>();
            ModelTransform = GetComponent<ModelLocator>()?.modelTransform;
            if (ModelTransform) BaseScale = ModelTransform.localScale;
            SelfCamera = GetComponent<CameraTargetParams>();
        }

        private void LateUpdate()
        {
            if (!SelfBody || !NetworkServer.active) return;

            int buffCount = SelfBody.GetBuffCount(StoneGrowthBuff.BuffDef);
            GrowthMod = Mathf.Lerp(GrowthMod, buffCount * StoneFluxRework.Size_Modifier.Value / 50f, Time.deltaTime * 10);

            if (ModelTransform) ModelTransform.localScale = BaseScale * (1f + GrowthMod);

            if (SelfCamera)
            {
                if (!HasAim && buffCount > StoneFluxRework.Estimated_Max / 4)
                {
                    HasAim = true;
                    AimRequest = SelfCamera.RequestAimType(CameraTargetParams.AimType.ZoomedOut);
                }
                else if (HasAim && AimRequest != null)
                {
                    HasAim = false;
                    AimRequest.Dispose();
                }
            }
        }
    }
}