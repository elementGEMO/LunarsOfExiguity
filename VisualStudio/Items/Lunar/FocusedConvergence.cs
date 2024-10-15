using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Skills;
using RoR2;
using R2API;
using System;
using UnityEngine;
using UnityEngine.Networking;

using static LunarsOfExiguity.ColorCode;
using static LunarsOfExiguity.ItemUtils;
using R2API.Networking;
using System.Collections.Generic;
using System.Linq;

namespace LunarsOfExiguity
{
    public class FocusedConvergence(bool configValue = true) : ItemBaseRework(configValue)
    {
        protected override string Token => "ITEM_FOCUSEDCONVERGENCE_";
        private static DamageColorIndex ConvergenceDamage;

        public static string Internal = "Focused Convergence";
        public static ConfigEntry<bool> Enable_Rework;
        public static ConfigEntry<float> Charge_Speed_Percent;
        public static ConfigEntry<int> Max_Damage_Percent;
        public static ConfigEntry<int> Percent_Loss_Hit;

        protected override void LanguageTokens()
        {
            if (MainConfig.RelicNameRewrite.Value == MainConfig.RewriteOptions.RelicRewrite) LanguageAPI.Add(Token + "NAME", "Relic of Focus");
            //LanguageAPI.Add(Token + "PICKUP", "Equipments no longer use charge... " + "BUT activating your Equipment disables all skills temporarily".Style(FontColor.cDeath) + ".");
            LanguageAPI.Add(Token + "DESC", string.Format(
                "Teleporters charge {0}% faster, but all enemies are invincible and until Teleporter is 99% charged. At 99% charge, enemies lose {1}% health, but taking damage during charge reduces final damage by {2}% per hit.",
                RoundVal(Charge_Speed_Percent.Value), RoundVal(Max_Damage_Percent.Value), RoundVal(Percent_Loss_Hit.Value)
            ));
        }
        protected override void DisabledTokens()
        {
            if (MainConfig.RelicNameRewrite.Value == MainConfig.RewriteOptions.RelicRewrite) LanguageAPI.Add(Token + "NAME", "Relic of Focus");
            /*
            if (MainConfig.RelicNameRewrite.Value == MainConfig.RewriteOptions.RelicRewrite) LanguageAPI.Add(Token + "NAME", "Relic of the Drowned");
            LanguageAPI.Add(Token + "DESC", "Reduce Equipment cooldown ".Style(FontColor.cIsUtility) + "by " + "50%".Style(FontColor.cIsUtility) + ". Forces your Equipment to " + "activate ".Style(FontColor.cIsUtility) + "whenever it is off " + "cooldown".Style(FontColor.cIsUtility) + ".");
            */
        }
        protected override void Methods()
        {
            ConvergenceDamage = ColorsAPI.RegisterDamageColor(new Color(0.278f, 0.184f, 0.925f));

            IL.RoR2.HoldoutZoneController.DoUpdate += DamageAll;
            On.RoR2.HoldoutZoneController.Start += DisableConvergence;
            IL.RoR2.HealthComponent.TakeDamageProcess += IncreaseDamageCounter;
            CharacterBody.onBodyAwakeGlobal += Invincibility;
        }
        private static void DamageAll(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdfld<HoldoutZoneController>(nameof(HoldoutZoneController.onCharged))
            ))
            {
                cursor.Emit(OpCodes.Ldarg, 0);
                cursor.EmitDelegate<Action<HoldoutZoneController>>(self =>
                {
                    int itemCount = Util.GetItemCountGlobal(RoR2Content.Items.FocusConvergence.itemIndex, true, false);
                    if (itemCount > 0)
                    {
                        int totalDamageInst = 0;
                        foreach (TeamComponent player in TeamComponent.GetTeamMembers(TeamIndex.Player).ToList())
                        {
                            HealthComponent health = player.GetComponent<HealthComponent>();
                            if (health && health.alive)
                            {
                                InvincibleDuringHoldout component = health.body.GetComponent<InvincibleDuringHoldout>();
                                if (component) totalDamageInst += component.DamageInstances;
                            }
                        }

                        float finalDamage = (Max_Damage_Percent.Value / 100f) * Mathf.Pow((1f - Percent_Loss_Hit.Value / 100f), totalDamageInst / itemCount) * itemCount;

                        foreach (TeamComponent nonPlayer in TeamComponent.GetTeamMembers(TeamIndex.Monster).ToList())
                        {
                            HealthComponent health = nonPlayer.GetComponent<HealthComponent>();
                            if (health && health.alive)
                            {
                                DamageInfo setDamage = new()
                                {
                                    attacker = null,
                                    procCoefficient = 0,
                                    damageType = (DamageType.BypassArmor | DamageType.BypassOneShotProtection | DamageType.BypassBlock),
                                    damageColorIndex = ConvergenceDamage,
                                    damage = health.combinedHealth * finalDamage
                                };
                                EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.runicMeteorEffect, new EffectData
                                {
                                    origin = health.body.transform.position,
                                    scale = 2f
                                }, true);
                                health.body.RemoveOldestTimedBuff(RoR2Content.Buffs.Immune);
                                health.TakeDamage(setDamage);
                            }
                        }
                    }
                });
            }
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
                    if (self.alive && damageInfo.damage > 0 && TeleporterInteraction.instance.isCharging)
                    {
                        CharacterBody bodyComponent = self.body;
                        Inventory inventory = bodyComponent ? bodyComponent.inventory : null;

                        if (inventory)
                        {
                            InvincibleDuringHoldout component = bodyComponent.GetComponent<InvincibleDuringHoldout>();
                            if (component && inventory.GetItemCount(RoR2Content.Items.FocusConvergence) > 0) component.DamageInstances++;
                        }
                    }
                });
            }
        }
        private static void Invincibility(CharacterBody self) => self.gameObject.AddComponent<InvincibleDuringHoldout>();
    }
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
            if (itemCount > 0) rate *= 1f + (Mathf.Pow(FocusedConvergence.Charge_Speed_Percent.Value / 100f, 1/itemCount));
        }
        private void Update()
        {
            VisualUpdate(Time.deltaTime);
        }
        private void VisualUpdate(float deltaTime)
        {
            itemCount = Util.GetItemCountForTeam(zoneController.chargingTeam, RoR2Content.Items.FocusConvergence.itemIndex, true, false);
            if (enabledTime.timeSince < HoldoutZoneController.FocusConvergenceController.startupDelay) itemCount = 0;
            float lerp = Mathf.MoveTowards(currentValue, itemCount > 0 ? 1f : 0f, 1f * deltaTime);
            if (currentValue <= 0f && lerp > 0f) Util.PlaySound("Play_item_lunar_focusedConvergence", gameObject);
            currentValue = lerp;
        }
    }
}
