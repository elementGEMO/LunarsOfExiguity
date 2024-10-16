using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace LunarsOfExiguity.Content.Lunar.Reworks;

public class FocusedConvergenceRework : ItemReworkBase
{
    public static DamageColorIndex DamageColorIndex = ColorsAPI.RegisterDamageColor(new Color(0.278f, 0.184f, 0.925f));
    
    protected override string Name => "FocusConvergence";

    protected override string RelicNameOverride => "Relic of Focus";
    protected override string PickupOverride => "Increase the speed of Teleporter charging... BUT all enemies are invincible for it's duration.";
    protected override string DescriptionOverride => $"Teleporters charge <style=cIsUtility>{LoEConfig.AdditionalChargeSpeedPercentage.Value}%</style> faster, but enemies are <style=cIsHealing>invulnerable</style> during it's duration. Once complete, enemies take <style=cIsDamage>{LoEConfig.MaxDamagePercentage.Value}%</style> of their <style=cIsHealing>health</style> in <style=cIsDamage>damage</style>, reduced by <style=cIsDamage>{LoEConfig.DamageLossOnHitPercentage.Value}%</style> for each time you were hit.";

    protected override void Initialize()
    {
        IL.RoR2.HoldoutZoneController.DoUpdate += DamageAll;
        On.RoR2.HoldoutZoneController.Start += DisableConvergence;
        IL.RoR2.HealthComponent.TakeDamageProcess += IncreaseDamageCounter;
        CharacterBody.onBodyAwakeGlobal += Invincibility;
    }

    private static void DamageAll(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (cursor.TryGotoNext(x => x.MatchLdfld<HoldoutZoneController>(nameof(HoldoutZoneController.onCharged))))
        {
            cursor.EmitDelegate(() =>
            {
                int itemCount = Util.GetItemCountGlobal(RoR2Content.Items.FocusConvergence.itemIndex, true, false);
                if (itemCount == 0) return;

                int totalDamageInstances = 0;
                foreach (TeamComponent playerTeamComponent in TeamComponent.GetTeamMembers(TeamIndex.Player))
                {
                    HealthComponent healthComponent = playerTeamComponent.body?.healthComponent;
                    if (!healthComponent || !healthComponent.alive) continue;

                    var component = healthComponent.body.GetComponent<InvincibleDuringHoldout>();
                    if (component) totalDamageInstances += component.DamageInstances;
                }

                float finalDamageMultiplier = LoEConfig.MaxDamagePercentage.Value * 0.01f *
                                              Mathf.Pow(1f - LoEConfig.DamageLossOnHitPercentage.Value / 0.01f,
                                                  totalDamageInstances / itemCount) * itemCount;
                foreach (TeamComponent monsterTeamComponent in TeamComponent.GetTeamMembers(TeamIndex.Monster))
                {
                    HealthComponent healthComponent = monsterTeamComponent.body?.healthComponent;
                    if (!healthComponent || !healthComponent.alive) continue;

                    EffectManager.SpawnEffect(EntityStates.Missions.BrotherEncounter.Phase1.centerOrbDestroyEffect,
                        new EffectData
                        {
                            origin = healthComponent.body.corePosition,
                            rotation = UnityEngine.Random.rotation,
                            scale = finalDamageMultiplier
                        }, true);

                    healthComponent.body.RemoveOldestTimedBuff(RoR2Content.Buffs.Immune);
                    healthComponent.TakeDamage(new DamageInfo
                    {
                        procCoefficient = 0,
                        damageType = DamageType.BypassArmor | DamageType.BypassOneShotProtection |
                                     DamageType.BypassBlock,
                        damageColorIndex = DamageColorIndex,
                        damage = healthComponent.fullCombinedHealth * finalDamageMultiplier
                    });
                }
            });
            return;
        }
        
        LoELog.Error("Failed to Apply FocusConvergenceRework HoldoutZoneController.DoUpdate Hook.");
    }
    
    private static void DisableConvergence(On.RoR2.HoldoutZoneController.orig_Start orig, HoldoutZoneController self)
    {
        self.gameObject.AddComponent<FasterDuration>();
        self.applyFocusConvergence = false;

        orig(self);
    }
    
    private static void IncreaseDamageCounter(ILContext il)
    {
        ILCursor cursor = new(il);

        if (cursor.TryGotoNext(
                x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.damageType)),
                x => x.MatchLdcI4(out _),
                x => x.MatchCallOrCallvirt<DamageTypeCombo>("op_Implicit"),
                x => x.MatchCallOrCallvirt<DamageTypeCombo>("op_BitwiseAnd"),
                x => x.MatchCallOrCallvirt<DamageTypeCombo>("op_Implicit"),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.Slow50))
            ))
        {
            cursor.Emit(OpCodes.Ldarg, 0);
            cursor.Emit(OpCodes.Ldarg, 1);

            cursor.EmitDelegate<Action<HealthComponent, DamageInfo>>((self, damageInfo) =>
            {
                if (!self.alive || damageInfo.damage > 0 || !TeleporterInteraction.instance || !TeleporterInteraction.instance.isCharged) return;
  
                if (self.body?.inventory)
                {
                    var component = self.GetComponent<InvincibleDuringHoldout>();
                    if (component && self.body.inventory.GetItemCount(RoR2Content.Items.FocusConvergence) > 0) component.DamageInstances++;
                }
            });
            return;
        } 
        LoELog.Error("Failed to Apply FocusConvergenceRework HealthComponent.TakeDamageProcess Hook.");
    }
    
    private static void Invincibility(CharacterBody self) => self.gameObject.AddComponent<InvincibleDuringHoldout>();
    
    
    public class InvincibleDuringHoldout : NetworkBehaviour
    {
        private CharacterBody Self;
        public int DamageInstances;
        private void Awake() => Self = GetComponent<CharacterBody>();
        private void FixedUpdate()
        {
            if (Self)
            {
                bool hasItem = Util.GetItemCountGlobal(RoR2Content.Items.FocusConvergence.itemIndex, true, false) > 0;
                if (hasItem)
                {
                    var holdoutInstance = TeleporterInteraction.instance;
                    if (holdoutInstance.isCharging && holdoutInstance.chargeFraction < 1)
                    {
                        if (Self.teamComponent && Self.teamComponent.teamIndex != TeamIndex.Player)
                        {
                            Self.AddTimedBuff(RoR2Content.Buffs.Immune, 1);
                        }
                    }
                }
            }
        }
    }
    
    
    public class FasterDuration : MonoBehaviour
    {
        private HoldoutZoneController zoneController;
        private Run.FixedTimeStamp enabledTime;
        private float currentValue;
        private int itemCount;
        private void Awake() => zoneController = GetComponent<HoldoutZoneController>();
        private void OnEnable()
        {
            enabledTime = Run.FixedTimeStamp.now;
            zoneController.calcChargeRate += ApplyRate;
            zoneController.calcColor += ApplyColor;
        }
        private void OnDisable()
        {
            zoneController.calcColor -= ApplyColor;
            zoneController.calcChargeRate -= ApplyRate;
        }
        private void ApplyColor(ref Color color)
        {
            color = Color.Lerp(color, HoldoutZoneController.FocusConvergenceController.convergenceMaterialColor, HoldoutZoneController.FocusConvergenceController.colorCurve.Evaluate(currentValue));
        }
        private void ApplyRate(ref float rate)
        {
            if (itemCount > 0) rate *= 1f + Mathf.Pow(LoEConfig.AdditionalChargeSpeedPercentage.Value * 0.01f, 1 / itemCount);
        }
        
        private void Update()
        {
            itemCount = Util.GetItemCountForTeam(zoneController.chargingTeam, RoR2Content.Items.FocusConvergence.itemIndex, true, false);
            if (enabledTime.timeSince < HoldoutZoneController.FocusConvergenceController.startupDelay) itemCount = 0;
            float lerp = Mathf.MoveTowards(currentValue, itemCount > 0 ? 1f : 0f, 0.5f * Time.deltaTime);
            if (currentValue <= 0f && lerp > 0f) Util.PlaySound("Play_item_lunar_focusedConvergence", gameObject);
            currentValue = lerp;
        }
    }
}