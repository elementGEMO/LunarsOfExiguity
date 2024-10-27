using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using R2API;

namespace LunarsOfExiguity;
public class FocusConvergenceHooks
{
    private static readonly string InternalName = "FocusConvergenceHooks";
    public static bool ReworkItemEnabled;
    public static bool PureItemEnabled;

    private static readonly DamageColorIndex FocusDamage = ColorsAPI.RegisterDamageColor(new Color32(255, 86, 131, 255));
    private struct PlayerDamageInstances
    {
        public int Instances;
        public CharacterBody Player;
        public ItemIndex Item;
    }

    public FocusConvergenceHooks()
    {
        ReworkItemEnabled = FocusedConvergenceRework.Rework_Enabled.Value;
        PureItemEnabled = PureFocusItem.Item_Enabled.Value;

        if (ReworkItemEnabled)
        {
            On.RoR2.HoldoutZoneController.Start += DisableConvergence;
            CharacterBody.onBodyAwakeGlobal += Invincibility;
        }
        if (ReworkItemEnabled || PureItemEnabled)
        {
            IL.RoR2.HoldoutZoneController.DoUpdate += DamageAll;
            IL.RoR2.HealthComponent.TakeDamageProcess += IncreaseDamageCounter;
        }
    }

    private static void DisableConvergence(On.RoR2.HoldoutZoneController.orig_Start orig, HoldoutZoneController self)
    {
        self.gameObject.AddComponent<FasterDuration>();
        self.applyFocusConvergence = false;

        orig(self);
    }
    private static void Invincibility(CharacterBody self) => self.gameObject.AddComponent<InvincibleDuringHoldout>();
    private static void DamageAll(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (cursor.TryGotoNext(x => x.MatchLdfld<HoldoutZoneController>(nameof(HoldoutZoneController.onCharged))))
        {
            cursor.EmitDelegate(() =>
            {
                bool hasFocus = ReworkItemEnabled && Util.GetItemCountGlobal(RoR2Content.Items.FocusConvergence.itemIndex, true, false) > 0;
                bool hasPure = PureItemEnabled && Util.GetItemCountGlobal(PureFocusItem.ItemDef.itemIndex, true, false) > 0;

                if (hasFocus || hasPure)
                {
                    List<PlayerDamageInstances> allPlayers = [];

                    foreach (TeamComponent playerComponent in TeamComponent.GetTeamMembers(TeamIndex.Player))
                    {
                        HealthComponent healthComponent = playerComponent.body?.healthComponent;
                        if (!healthComponent || !healthComponent.alive) continue;

                        Inventory inventory = playerComponent.body.inventory;
                        if (!inventory) continue;

                        PlayerDamageInstances damageInstance = new()
                        {
                            Instances = playerComponent.body.GetBuffCount(FocusCounterBuff.BuffDef),
                            Player = playerComponent.body,
                            Item = ItemIndex.None
                        };

                        if (damageInstance.Instances > 0)
                        {
                            if (ReworkItemEnabled && inventory.GetItemCount(RoR2Content.Items.FocusConvergence) > 0)
                            {
                                damageInstance.Item = RoR2Content.Items.FocusConvergence.itemIndex;
                            }
                            else if (PureItemEnabled && inventory.GetItemCount(PureFocusItem.ItemDef) > 0)
                            {
                                damageInstance.Item = PureFocusItem.ItemDef.itemIndex;
                            }

                            if (damageInstance.Item != ItemIndex.None) allPlayers.Add(damageInstance);
                        }
                    }

                    foreach (TeamComponent monsterComponent in TeamComponent.GetTeamMembers(TeamIndex.Monster))
                    {
                        HealthComponent healthComponent = monsterComponent.body?.healthComponent;
                        if (!healthComponent || !healthComponent.alive) continue;

                        EffectManager.SpawnEffect(EntityStates.Missions.BrotherEncounter.Phase1.centerOrbDestroyEffect, new EffectData()
                        {
                            origin = healthComponent.body.corePosition,
                            rotation = UnityEngine.Random.rotation,
                            scale = healthComponent.body.radius
                        }, true);

                        healthComponent.body.RemoveOldestTimedBuff(RoR2Content.Buffs.Immune);
                        healthComponent.body.RemoveOldestTimedBuff(RoR2Content.Buffs.HiddenInvincibility);

                        foreach (PlayerDamageInstances damageInstance in allPlayers)
                        {
                            if (!healthComponent || !healthComponent.alive) break;

                            float damageMod = 0;
                            if (ReworkItemEnabled && damageInstance.Item == RoR2Content.Items.FocusConvergence.itemIndex)
                            {
                                damageMod = FocusedConvergenceRework.Max_Damage_Percent.Value / 100f * Mathf.Pow(1f - FocusedConvergenceRework.Percent_Loss_Hit.Value / 100f, damageInstance.Instances);
                            }
                            else if (PureItemEnabled && damageInstance.Item == PureFocusItem.ItemDef.itemIndex)
                            {
                                damageMod = PureFocusItem.Max_Damage_Percent.Value / 100f * Mathf.Pow(1f - PureFocusItem.Percent_Loss_Hit.Value / 100f, damageInstance.Instances);
                            }
                            damageInstance.Player.RemoveBuff(FocusCounterBuff.BuffDef);

                            Log.Debug("Damage: " +  damageMod);

                            healthComponent.TakeDamage(new DamageInfo
                            {
                                attacker = damageInstance.Player.gameObject,
                                position = healthComponent.body.transform.position,
                                procCoefficient = 0,
                                damageType = DamageType.BypassArmor | DamageType.BypassOneShotProtection | DamageType.BypassBlock,
                                damageColorIndex = FocusDamage,
                                damage = healthComponent.fullCombinedHealth * damageMod
                            });
                        }
                    }
                }
            });
        }
        else Log.Warning(InternalName + " - #1 (DamageAll) Failure");
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
                if (self.alive && damageInfo.damage > 0 && TeleporterInteraction.instance && TeleporterInteraction.instance.isCharging)
                {
                    if (self.body?.inventory)
                    {
                        if (ReworkItemEnabled && self.body.inventory.GetItemCount(RoR2Content.Items.FocusConvergence) > 0) self.body.AddBuff(FocusCounterBuff.BuffDef);
                        else if (PureItemEnabled && self.body.inventory.GetItemCount(PureFocusItem.ItemDef) > 0) self.body.AddBuff(FocusCounterBuff.BuffDef);
                    }
                }
            });
        }
        else Log.Warning(InternalName + " - #1 (IncreaseDamageCounter) Failure");
    }

    public class InvincibleDuringHoldout : NetworkBehaviour
    {
        private CharacterBody Self;
        private void Awake()
        {
            if (!GetComponent<HealthComponent>() || !GetComponent<HealthComponent>().alive) Destroy(GetComponent<InvincibleDuringHoldout>());
            else Self = GetComponent<CharacterBody>();
        }
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
            if (itemCount > 0) rate *= 1f + Mathf.Pow(FocusedConvergenceRework.Charge_Speed_Percent.Value * 0.01f, 1 / itemCount);
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